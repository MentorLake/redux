using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Channels;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Middleware;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace MentorLake.Redux;

public record ActionDispatch
{
	public object[] Actions { get; init; }
	public TaskCompletionSource CompletionSource { get; init; }
}

public sealed partial class ReduxStore : IAsyncDisposable
{
	private readonly Subject<object> _actionDispatcher = new();
	private readonly List<ActionReducer<StoreState>> _reducers = new();
	private readonly List<IReduxMiddleware> _middlewares = new();
	private readonly BehaviorSubject<StoreState> _stateSubject;
	private NextMiddleware _middlewarePipeline;
	private int _processorThreadId = -1;
	private readonly Channel<ActionDispatch> _actionChannel = Channel.CreateUnbounded<ActionDispatch>(new() { SingleReader = true });
	private readonly CancellationTokenSource _processorCancellationToken = new();
	private readonly Task _processorTask;
	private bool _isDisposed;
	private readonly Subject<Exception> _errorSubject = new();

	public IObservable<Exception> Errors => _errorSubject;
	public StoreState State { get; private set; }
	public IObservable<object> Actions => _actionDispatcher;

	public ReduxStore()
	{
		State = new();
		_stateSubject = new(State);
		_processorTask = Task.Run(() => ProcessActionsAsync(_processorCancellationToken.Token));
	}

	public async ValueTask DisposeAsync()
	{
		Volatile.Write(ref _isDisposed, true);
		_actionChannel.Writer.TryComplete();
		await _processorTask;
		_processorCancellationToken.Dispose();
		_actionDispatcher.Dispose();
		_stateSubject.Dispose();
		_errorSubject.Dispose();
	}

	public void Dispatch(params object[] actions)
	{
		if (Volatile.Read(ref _isDisposed)) throw new ObjectDisposedException(nameof(ReduxStore));
		if (actions == null) return;
		var filtered = actions.Where(a => a != null).ToArray();
		if (filtered.Length == 0) return;
		DispatchInternal(filtered);
	}

	private void DispatchInternal(object[] actions)
	{
		var taskCompletionSource = new TaskCompletionSource();

		if (!_actionChannel.Writer.TryWrite(new() { Actions = actions, CompletionSource = taskCompletionSource }))
		{
			throw new ObjectDisposedException(nameof(ReduxStore));
		}

		if (Volatile.Read(ref _processorThreadId) != Environment.CurrentManagedThreadId)
		{
			taskCompletionSource.Task.GetAwaiter().GetResult();
		}
	}

	private async Task ProcessActionsAsync(CancellationToken cancellationToken = default)
	{
		var reader = _actionChannel.Reader;

		while (await reader.WaitToReadAsync(cancellationToken))
		{
			Interlocked.Exchange(ref _processorThreadId, Environment.CurrentManagedThreadId);

			while (reader.TryRead(out var action))
			{
				try
				{
					var initialState = State;
					var middlewarePipeline = _middlewarePipeline ??= BuildMiddlewarePipeline();

					try
					{
						foreach (var a in action.Actions)
						{
							middlewarePipeline(a);
						}
					}
					catch (Exception e)
					{
						if (!ReferenceEquals(initialState, State))
						{
							_stateSubject.OnNext(State);
						}

						_errorSubject.OnNext(e);
						action.CompletionSource.SetException(e);
						continue;
					}

					if (!ReferenceEquals(initialState, State))
					{
						_stateSubject.OnNext(State);
					}

					action.CompletionSource.SetResult();
				}
				catch (Exception ex)
				{
					_errorSubject.OnNext(ex);
					action.CompletionSource.SetException(ex);
				}
			}

			Interlocked.Exchange(ref _processorThreadId, -1);
		}
	}

	private void ReduceAndNotify(object action)
	{
		State = Reduce(State, action);
		_actionDispatcher.OnNext(action);
	}

	private StoreState Reduce(StoreState state, object action)
	{
		var actionName = action is IAction a ? a.ActionName : action.GetType().FullName;
		var currentState = state;

		foreach (var reducer in _reducers)
		{
			if (reducer.ActionType == actionName)
			{
				currentState = reducer.Reduce(currentState, action);
			}
		}

		return currentState;
	}

	public void UseMiddleware(params IReduxMiddleware[] middlewares)
	{
		ArgumentNullException.ThrowIfNull(middlewares);
		foreach (var middleware in middlewares)
		{
			ArgumentNullException.ThrowIfNull(middleware);
			middleware.Initialize(this);
			_middlewares.Add(middleware);
		}

		_middlewarePipeline = null;
	}

	private NextMiddleware BuildMiddlewarePipeline()
	{
		NextMiddleware pipeline = ReduceAndNotify;
		for (var i = _middlewares.Count - 1; i >= 0; i--)
		{
			var middleware = _middlewares[i];
			var next = pipeline;
			pipeline = action => middleware.Invoke(action, next);
		}

		return pipeline;
	}

	public void RegisterEffects(params IEffectsFactory[] factories)
	{
		RegisterEffects(factories.SelectMany(f => f.Create()).ToArray());
	}

	public void RegisterEffects(params Effect[] effects)
	{
		effects
			.Where(effect => effect.Run != null && effect.Config != null)
			.Select(effect => effect.Config.Dispatch
				? effect.Run(Actions).Retry()
				: effect.Run(Actions).Retry().Select(_ => (object)null))
			.Merge()
			.Where(a => a != null)
			.Subscribe(a => Dispatch(a));
	}

	public void RegisterReducers(params IReducerFactory[] reducerFactories)
	{
		foreach (var factory in reducerFactories)
		{
			RegisterReducers(factory.Create());
		}
	}

	public void RegisterReducers(params FeatureReducerCollection[] reducerCollections)
	{
		foreach (var collection in reducerCollections)
		{
			foreach (var r in collection)
			{
				RegisterReducers(r);
			}
		}
	}

	public void RegisterReducers(params IFeatureReducer[] reducers)
	{
		foreach (var r in reducers)
		{
			State = r.InitializeStore(State);
			_reducers.AddRange(r.ActionReducers);
		}

		_stateSubject.OnNext(State);
	}

	public void RegisterAssembly(Assembly assembly)
	{
		if (assembly == null) throw new ArgumentNullException(nameof(assembly));

		var factoryTypes = assembly.GetTypes()
			.Where(t => typeof(IReducerFactory).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);
		foreach (var ft in factoryTypes)
		{
			var factory = (IReducerFactory)Activator.CreateInstance(ft);
			RegisterReducers(factory);
		}

		var reducerTypes = assembly.GetTypes()
			.Where(t => typeof(IFeatureReducer).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);
		foreach (var rt in reducerTypes)
		{
			var reducer = (IFeatureReducer)Activator.CreateInstance(rt);
			RegisterReducers(reducer);
		}

		var effectFactoryTypes = assembly.GetTypes()
			.Where(t => typeof(IEffectsFactory).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);
		foreach (var eft in effectFactoryTypes)
		{
			var factory = (IEffectsFactory)Activator.CreateInstance(eft);
			RegisterEffects(factory);
		}
	}

	public IObservable<TResult> Select<TResult>(Func<StoreState, TResult> selector)
	{
		return _stateSubject.Select(selector).DistinctUntilChanged();
	}

	public IObservable<TResult> Select<TResult>(ISelector<TResult> selector)
	{
		return selector.Apply(_stateSubject);
	}
}
