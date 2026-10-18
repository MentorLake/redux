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

	/// <summary>
	/// Stream of exceptions that escaped the dispatch pipeline (middleware, reducers,
	/// or the processor itself). Failures are also routed to the dispatching caller.
	/// </summary>
	public IObservable<Exception> Errors => _errorSubject;

	/// <summary>
	/// The current state snapshot. Always reflects every dispatch that has completed;
	/// see <see cref="Dispatch"/> for the settlement guarantee.
	/// </summary>
	public StoreState State { get; private set; }

	/// <summary>
	/// Stream of every action as it passes through the pipeline, on the processor thread.
	/// Subscribe with <c>ObserveOn</c> or a bounded strategy for nontrivial handlers
	/// (see docs/slow-consumers.md).
	/// </summary>
	public IObservable<object> Actions => _actionDispatcher;

	/// <summary>
	/// Creates a store with empty state and starts the background action processor.
	/// Register reducers and effects before dispatching.
	/// </summary>
	public ReduxStore()
	{
		State = new();
		_stateSubject = new(State);
		_processorTask = Task.Run(() => ProcessActionsAsync(_processorCancellationToken.Token));
	}

	/// <summary>
	/// Stops the processor, releases any waiting dispatchers, and disposes the internal
	/// observables. Awaits the processor task, so all actions already queued are fully
	/// processed before this returns. Dispatching after disposal throws
	/// <see cref="ObjectDisposedException"/>.
	/// </summary>
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

	/// <summary>
	/// Dispatches one or more actions through the pipeline atomically: middleware,
	/// reducers, the <see cref="Actions"/> stream, and a single state notification.
	/// </summary>
	/// <param name="actions">The actions to apply. Null entries are ignored.</param>
	/// <remarks>
	/// <para>
	/// Blocking by design: when this returns from a non-processor thread, <em>these</em>
	/// actions are fully settled and <see cref="State"/> reflects them. The contract is
	/// per-dispatch, not per-cascade — actions re-dispatched as a consequence (effects
	/// with <c>Dispatch = true</c>) are queued and may settle after this call returns.
	/// Observe consequences via <see cref="Actions"/> or <see cref="Select{TResult}(Func{StoreState, TResult})"/>.
	/// </para>
	/// <para>
	/// Called from the processor thread (inside middleware or a reducer), the actions are
	/// applied inline without waiting, which keeps re-entrant dispatch deadlock-free.
	/// </para>
	/// </remarks>
	/// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
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

	/// <summary>
	/// Adds middleware to the pipeline, wrapping reduce-and-notify in registration order.
	/// Call before dispatching; middleware must be synchronous and fast — it runs on the
	/// processor thread.
	/// </summary>
	/// <param name="middlewares">The middleware to install. Each is initialized once.</param>
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

	/// <summary>
	/// Creates and registers all effects produced by the given factories.
	/// </summary>
	/// <param name="factories">Effect factories; each <see cref="IEffectsFactory.Create"/> is invoked once.</param>
	public void RegisterEffects(params IEffectsFactory[] factories)
	{
		RegisterEffects(factories.SelectMany(f => f.Create()).ToArray());
	}

	/// <summary>
	/// Subscribes the given effects to the <see cref="Actions"/> stream. Effects whose
	/// config sets <c>Dispatch = true</c> re-dispatch their outputs; others are observed
	/// only. Faulted effect streams are retried.
	/// </summary>
	/// <param name="effects">The effects to run.</param>
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

	/// <summary>
	/// Creates and registers all reducer collections produced by the given factories.
	/// </summary>
	/// <param name="reducerFactories">Reducer factories; each <see cref="IReducerFactory.Create"/> is invoked once.</param>
	public void RegisterReducers(params IReducerFactory[] reducerFactories)
	{
		foreach (var factory in reducerFactories)
		{
			RegisterReducers(factory.Create());
		}
	}

	/// <summary>
	/// Registers every feature reducer in the given collections.
	/// </summary>
	/// <param name="reducerCollections">Collections built by <c>FeatureReducer.Build(...)</c>.</param>
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

	/// <summary>
	/// Registers feature reducers: initializes each feature's state in the store and
	/// installs its action handlers. Re-registering a feature resets its state.
	/// </summary>
	/// <param name="reducers">The feature reducers to install.</param>
	public void RegisterReducers(params IFeatureReducer[] reducers)
	{
		foreach (var r in reducers)
		{
			State = r.InitializeStore(State);
			_reducers.AddRange(r.ActionReducers);
		}

		_stateSubject.OnNext(State);
	}

	/// <summary>
	/// Scans an assembly for parameterless-constructible <see cref="IReducerFactory"/>,
	/// <see cref="IFeatureReducer"/>, and <see cref="IEffectsFactory"/> implementations
	/// and registers everything it finds.
	/// </summary>
	/// <param name="assembly">The assembly to scan.</param>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is null.</exception>
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

	/// <summary>
	/// Observes a projection of the state: emits the current value immediately, then again
	/// only when the projected value changes (<c>DistinctUntilChanged</c>).
	/// </summary>
	/// <typeparam name="TResult">The projected type.</typeparam>
	/// <param name="selector">Projection applied to each <see cref="StoreState"/>.</param>
	/// <remarks>
	/// The selector and subscribers run on the processor thread. Marshal off with
	/// <c>ObserveOn</c> for nontrivial handlers; see docs/slow-consumers.md for
	/// coalescing strategies.
	/// </remarks>
	public IObservable<TResult> Select<TResult>(Func<StoreState, TResult> selector)
	{
		return _stateSubject.Select(selector).DistinctUntilChanged();
	}

	/// <summary>
	/// Observes a composed <see cref="ISelector{TResult}"/>: memoized, shared across
	/// subscribers, and emitting only on semantic change.
	/// </summary>
	/// <typeparam name="TResult">The selector's output type.</typeparam>
	/// <param name="selector">A selector built with <c>SelectorFactory</c>.</param>
	/// <remarks>
	/// The selector and subscribers run on the processor thread. Marshal off with
	/// <c>ObserveOn</c> for nontrivial handlers; see docs/slow-consumers.md for
	/// coalescing strategies.
	/// </remarks>
	public IObservable<TResult> Select<TResult>(ISelector<TResult> selector)
	{
		return selector.Apply(_stateSubject);
	}
}
