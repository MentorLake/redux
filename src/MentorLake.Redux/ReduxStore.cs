using System.Reflection;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Middleware;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace MentorLake.Redux;

public sealed partial class ReduxStore
{
	private readonly Subject<object> _actionDispatcher = new();
	private readonly List<ActionReducer<StoreState>> _reducers = new();
	private readonly List<IReduxMiddleware> _middlewares = new();
	private readonly BehaviorSubject<StoreState> _stateSubject;
	private NextMiddleware _middlewarePipeline;
	private int _batchDepth;
	private bool _batchStateChanged;

	public ReduxStore()
	{
		State = new();
		_stateSubject = new(State);
	}

	public StoreState State { get; private set; }
	public IObservable<object> Actions => _actionDispatcher;

	public void Dispatch(object action)
	{
		if (action == null) return;
		(_middlewarePipeline ??= BuildMiddlewarePipeline())(action);
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

	public void Batch(Action batch)
	{
		ArgumentNullException.ThrowIfNull(batch);

		try
		{
			_batchDepth++;
			batch();
		}
		finally
		{
			_batchDepth--;

			if (_batchDepth == 0 && _batchStateChanged)
			{
				_batchStateChanged = false;
				_stateSubject.OnNext(State);
			}
		}
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

	private void ReduceAndNotify(object action)
	{
		UpdateState(Reduce(State, action));
		_actionDispatcher.OnNext(action);
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

	private void UpdateState(StoreState state)
	{
		State = state;
		if (_batchDepth > 0)
		{
			_batchStateChanged = true;
			return;
		}

		_stateSubject.OnNext(state);
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
