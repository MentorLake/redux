namespace MentorLake.Redux.Reducers;

/// <summary>
/// Entry point for building a feature reducer:
/// <c>FeatureReducer.Build(new CounterState(0)).On&lt;IncrementAction&gt;(...)</c>.
/// </summary>
public static class FeatureReducer
{
	/// <summary>
	/// Starts a feature reducer with the given initial state. The state type is the
	/// feature's key — no other feature may use the same CLR type.
	/// </summary>
	/// <typeparam name="TFeatureState">The feature's state record type.</typeparam>
	/// <param name="initialState">The value the slice holds before any action runs.</param>
	public static FeatureReducer<TFeatureState> Build<TFeatureState>(TFeatureState initialState) where TFeatureState : class
	{
		return new FeatureReducer<TFeatureState>(initialState);
	}
}

/// <summary>
/// Owns one feature slice: its initial state and the handlers that transform it.
/// Handlers must be pure and return new instances when data changes.
/// </summary>
/// <typeparam name="TFeatureState">The feature's state record type (also its key).</typeparam>
public class FeatureReducer<TFeatureState>(TFeatureState initialState) : IFeatureReducer where TFeatureState : class
{
	private readonly List<ActionReducer<TFeatureState>> _actionReducers = new();

	/// <summary>
	/// Handles actions of CLR type <typeparamref name="TAction"/>.
	/// </summary>
	/// <typeparam name="TAction">The action type to match.</typeparam>
	/// <param name="f">Pure transformation from (slice, action) to the next slice.</param>
	public FeatureReducer<TFeatureState> On<TAction>(Func<TFeatureState, TAction, TFeatureState> f) where TAction : class
	{
		_actionReducers.Add(new ActionReducer<TFeatureState>
		{
			Reduce = (state, action) => action is TAction actionT1 ? f(state, actionT1) : state,
			ActionType = typeof(TAction).FullName
		});

		return this;
	}

	/// <summary>
	/// Handles actions whose <see cref="IAction.ActionName"/> equals <paramref name="actionName"/>.
	/// Essential for thunk lifecycle actions, which share a few CLR types.
	/// </summary>
	/// <typeparam name="TAction">The CLR type the matched actions are cast to.</typeparam>
	/// <param name="actionName">The name to match (e.g. <c>"LoadCounter/fulfilled"</c>).</param>
	/// <param name="f">Pure transformation from (slice, action) to the next slice.</param>
	public FeatureReducer<TFeatureState> On<TAction>(string actionName, Func<TFeatureState, TAction, TFeatureState> f) where TAction : class
	{
		_actionReducers.Add(new ActionReducer<TFeatureState>
		{
			Reduce = (state, action) => action is TAction actionT1 ? f(state, actionT1) : state,
			ActionType = actionName
		});

		return this;
	}

	public StoreState InitializeStore(StoreState state)
	{
		return state.UpdateFeatureState(initialState);
	}

	public IEnumerable<ActionReducer<StoreState>> ActionReducers => _actionReducers.Select(r => new ActionReducer<StoreState>()
	{
		Reduce = (state, action) =>
		{
			var featureState = state.GetFeatureState<TFeatureState>();
			var newFeatureState = r.Reduce(featureState, action);
			return state.UpdateFeatureState(newFeatureState);
		},
		ActionType = r.ActionType
	});
}
