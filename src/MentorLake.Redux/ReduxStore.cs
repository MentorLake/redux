using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace MentorLake.Redux;

public sealed class ReduxStore
{
	private readonly TaskScheduler _actionTaskScheduler;
	private readonly Subject<object> _actionDispatcher = new();
	private readonly List<ActionReducer<StoreState>> _reducers = new();
	private readonly BehaviorSubject<StoreState> _stateSubject;

	public ReduxStore(TaskScheduler dispatchedActionScheduler = null)
	{
		State = new StoreState();
		_stateSubject = new BehaviorSubject<StoreState>(State);
		_actionTaskScheduler = dispatchedActionScheduler ?? new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
	}

	public StoreState State { get; private set; }
	public IObservable<object> Actions => _actionDispatcher;

	public async Task Dispatch(object action)
	{
		if (action == null) return;
		await Task.Factory.StartNew(() => ProcessActionQueue(action), CancellationToken.None, TaskCreationOptions.None, _actionTaskScheduler);
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
		var actionName = action.GetType().FullName;
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
