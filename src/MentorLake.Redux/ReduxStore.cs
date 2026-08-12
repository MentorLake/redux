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

	public ReduxStore()
	{
		State = new StoreState();
		_stateSubject = new BehaviorSubject<StoreState>(State);
	}

	public StoreState State { get; private set; }
	public IObservable<object> Actions => _actionDispatcher;

	public void Dispatch(object action)
	{
		if (action == null) return;
		ProcessActionQueue(action);
	}

	public ThunkResult<TResult> DispatchThunk<TResult>(ICallableThunkFunc<TResult> thunk)
	{
		return new ThunkResult<TResult>() { Actions = CreateThunkObservable(thunk) };
	}

	public ThunkResult DispatchThunk(ICallableThunkAction thunk)
	{
		return new ThunkResult() { Actions = CreateThunkObservable(thunk) };
	}

	public ThunkResult DispatchThunk(Func<ThunkApi, Task> work)
	{
		return DispatchThunk(new CallableThunkAction(string.Empty, work));
	}

	public ThunkResult<TResult> DispatchThunk<TResult>(Func<ThunkApi, Task<TResult>> work)
	{
		return DispatchThunk(new CallableThunkFunc<TResult>(string.Empty, work));
	}

	private IObservable<object> CreateThunkObservable(ICallableThunkAction thunk)
	{
		return Observable.Create<object>(observer =>
		{
			var localActionDispatcher = new Subject<object>();
			var api = new ThunkApi(localActionDispatcher, this);

			var subscription = localActionDispatcher
				.Do(Dispatch)
				.TakeUntil(a => a is ThunkFulfilled or ThunkRejected)
				.Subscribe(observer);

			_ = thunk.ExecuteAsync(api);
			return subscription;
		});
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
