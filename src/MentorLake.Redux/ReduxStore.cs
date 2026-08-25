using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux;

public sealed partial class ReduxStore
{
	private readonly Subject<object> _actionDispatcher = new();
	private readonly List<ActionReducer<StoreState>> _reducers = new();
	private readonly BehaviorSubject<StoreState> _stateSubject;
	private readonly Dictionary<Type, Func<ThunkApiContext, ThunkApi>> _thunkApiFactories = new()
	{
		[typeof(ThunkApi)] = static ctx => new ThunkApi(ctx)
	};

	public ReduxStore()
	{
		State = new StoreState();
		_stateSubject = new BehaviorSubject<StoreState>(State);
	}

	public StoreState State { get; private set; }
	public IObservable<object> Actions => _actionDispatcher;

	public void UseThunkApi<TApi>(Func<ThunkApiContext, TApi> factory) where TApi : ThunkApi
	{
		ArgumentNullException.ThrowIfNull(factory);
		_thunkApiFactories[typeof(TApi)] = ctx => factory(ctx);
	}

	public ThunkDispatcher<TApi> Using<TApi>() where TApi : ThunkApi
	{
		return new ThunkDispatcher<TApi>(this);
	}

	public void Dispatch(object action)
	{
		if (action == null) return;
		Debug.WriteLine($"Dispatching action: {action.GetType().Name}");
		ProcessActionQueue(action);
	}

	public ThunkResult DispatchThunk<TApi>(ICallableThunkAction<TApi> thunk) where TApi : ThunkApi
	{
		return new ThunkResult { Actions = CreateThunkObservable(thunk) };
	}

	public ThunkResult<TResult> DispatchThunk<TApi, TResult>(ICallableThunkFunc<TApi, TResult> thunk) where TApi : ThunkApi
	{
		return new ThunkResult<TResult> { Actions = CreateThunkObservable(thunk) };
	}

	private IObservable<object> CreateThunkObservable<TApi>(ICallableThunkAction<TApi> thunk, TApi api = null) where TApi : ThunkApi
	{
		return Observable.Create<object>(observer =>
		{
			var localActionDispatcher = new Subject<object>();
			api ??= CreateThunkApi<TApi>(localActionDispatcher);

			var subscription = localActionDispatcher
				.Do(Dispatch)
				.TakeUntil(a => a is ThunkFulfilled or ThunkRejected)
				.Subscribe(observer);

			_ = thunk.ExecuteAsync(api);
			return subscription;
		});
	}

	private TApi CreateThunkApi<TApi>(Subject<object> actionDispatcher) where TApi : ThunkApi
	{
		var context = new ThunkApiContext(actionDispatcher, this);
		var apiType = typeof(TApi);

		if (_thunkApiFactories.TryGetValue(apiType, out var factory))
		{
			return (TApi)factory(context);
		}

		throw new InvalidOperationException($"No factory registered for {apiType.FullName}");
	}

	private void ProcessActionQueue(object action)
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

	private StoreState Reduce(StoreState state, object action)
	{
		var actionName = action is IAction a ? a.ActionName : action.GetType().FullName;
		var currentState = state;

		foreach (var reducer in _reducers)
		{
			if (reducer.ActionType.Contains(actionName))
			{
				currentState = reducer.Reduce(currentState, action);
			}
		}

		return currentState;
	}

	private void UpdateState(StoreState state)
	{
		State = state;
		_stateSubject.OnNext(State);
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
