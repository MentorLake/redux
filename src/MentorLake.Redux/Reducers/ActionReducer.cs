namespace MentorLake.Redux.Reducers;

/// <summary>
/// A single action handler for one feature slice: a reduce function keyed by action type name.
/// Built for you by <see cref="FeatureReducer.On{TAction}(Func{TFeatureState, TAction, TFeatureState})"/>.
/// </summary>
/// <typeparam name="TState">The state shape this reducer operates on.</typeparam>
public class ActionReducer<TState> where TState : class
{
	/// <summary>Pure transformation: (current slice, action) → next slice.</summary>
	public Func<TState, object, TState> Reduce { get; set; }

	/// <summary>Action identity this handler matches — CLR type full name or <see cref="IAction.ActionName"/>.</summary>
	public string ActionType { get; set; }
}
