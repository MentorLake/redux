using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux;

public sealed partial class ReduxStore
{
	private readonly Dictionary<string, ThunkGroup> _thunkGroups = new();
	private readonly Dictionary<Type, Func<ThunkApiContext, ThunkApi>> _thunkApiFactories = new()
	{
		[typeof(ThunkApi)] = static ctx => new(ctx)
	};

	/// <summary>
	/// Registers the factory used to create a custom <see cref="ThunkApi"/> subtype,
	/// enabling services (via constructor injection in the factory) for thunks dispatched
	/// through <see cref="Using{TApi}"/>.
	/// </summary>
	/// <typeparam name="TApi">The custom thunk API type.</typeparam>
	/// <param name="factory">Creates an API instance per thunk run from its context.</param>
	public void RegisterThunkApiFactory<TApi>(Func<ThunkApiContext, TApi> factory) where TApi : ThunkApi
	{
		ArgumentNullException.ThrowIfNull(factory);
		_thunkApiFactories[typeof(TApi)] = ctx => factory(ctx);
	}

	/// <summary>
	/// Entry point for dispatching thunks against a specific <see cref="ThunkApi"/> type.
	/// </summary>
	/// <typeparam name="TApi">The API type; must have a registered factory (see
	/// <see cref="RegisterThunkApiFactory{TApi}"/>).</typeparam>
	public ThunkDispatcher<TApi> Using<TApi>() where TApi : ThunkApi
	{
		return new(this);
	}

	private static CancellationTokenSource CreateOwnedCancellation(CancellationToken cancellationToken, out CancellationToken token)
	{
		var owned = cancellationToken.CanBeCanceled
			? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
			: new();

		token = owned.Token;
		return owned;
	}

	private ThunkGroup GetOrCreateThunkGroup(string cancellationGroup)
	{
		ArgumentNullException.ThrowIfNull(cancellationGroup);

		if (!_thunkGroups.TryGetValue(cancellationGroup, out var group))
		{
			group = new(cancellationGroup);
			_thunkGroups[cancellationGroup] = group;
		}

		return group;
	}

	private IObservable<object> StartThunk<TApi>(ICallableThunkAction<TApi> thunk, CancellationToken token, ThunkGroup group = null, CancellationTokenSource cancellation = null) where TApi : ThunkApi
	{
		var actions = new ReplaySubject<object>();
		var localActionDispatcher = new Subject<object>();
		var api = CreateThunkApi<TApi>(localActionDispatcher, token);

		localActionDispatcher
			.Do(a => Dispatch(a))
			.TakeUntil(a => a is ThunkFulfilled or ThunkRejected or ThunkCancelled)
			.Subscribe(actions);

		_ = RunThunk(thunk, api, group, cancellation);
		return actions;
	}

	private async Task RunThunk<TApi>(ICallableThunkAction<TApi> thunk, TApi api, ThunkGroup group, CancellationTokenSource cancellation) where TApi : ThunkApi
	{
		try
		{
			await thunk.ExecuteAsync(api);
		}
		finally
		{
			if (group != null)
			{
				group.End(cancellation);
			}
			else
			{
				cancellation?.Dispose();
			}
		}
	}

	private TApi CreateThunkApi<TApi>(Subject<object> actionDispatcher, CancellationToken cancellationToken) where TApi : ThunkApi
	{
		var context = new ThunkApiContext(actionDispatcher, this, cancellationToken);
		var apiType = typeof(TApi);

		if (_thunkApiFactories.TryGetValue(apiType, out var factory))
		{
			return (TApi)factory(context);
		}

		throw new InvalidOperationException($"No factory registered for {apiType.FullName}");
	}

	/// <summary>
	/// Runs a thunk action, dispatching <c>pending</c>/<c>fulfilled</c>/<c>rejected</c>
	/// lifecycle actions around it.
	/// </summary>
	/// <typeparam name="TApi">The thunk API type to run against.</typeparam>
	/// <param name="thunk">The callable thunk (typically <c>SomeThunk.Bind(...)</c>).</param>
	/// <param name="cancellationToken">Optional cancellation for the thunk run.</param>
	/// <returns>
	/// A <see cref="ThunkResult"/> exposing the lifecycle <see cref="Actions"/> stream and
	/// <c>ToTask()</c> for awaiting completion. The thunk starts when the result is
	/// subscribed or awaited.
	/// </returns>
	public ThunkResult DispatchThunk<TApi>(ICallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var owned = CreateOwnedCancellation(cancellationToken, out var token);
		return new(StartThunk(thunk, token, null, owned), owned);
	}

	/// <summary>
	/// Runs a thunk function that produces a result, dispatching lifecycle actions around it.
	/// </summary>
	/// <typeparam name="TApi">The thunk API type to run against.</typeparam>
	/// <typeparam name="TResult">The thunk's return type.</typeparam>
	/// <param name="thunk">The callable thunk function (typically <c>SomeThunk.Bind(...)</c>).</param>
	/// <param name="cancellationToken">Optional cancellation for the thunk run.</param>
	/// <returns>
	/// A <see cref="ThunkResult{TResult}"/> whose <c>ToTask()</c> yields the thunk's value
	/// (or rethrows its exception). The thunk starts when the result is subscribed or awaited.
	/// </returns>
	public ThunkResult<TResult> DispatchThunk<TApi, TResult>(ICallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var owned = CreateOwnedCancellation(cancellationToken, out var token);
		return new(StartThunk(thunk, token, null, owned), owned);
	}

	/// <summary>
	/// Runs a thunk action in a cancellation group: starting a new thunk in the same group
	/// cancels the previous one (search-as-you-type semantics).
	/// </summary>
	/// <typeparam name="TApi">The thunk API type to run against.</typeparam>
	/// <param name="cancellationGroup">Group name; latest dispatch per group wins.</param>
	/// <param name="thunk">The callable thunk (typically <c>SomeThunk.Bind(...)</c>).</param>
	/// <param name="cancellationToken">Optional cancellation combined with the group's.</param>
	/// <returns>
	/// A <see cref="ThunkResult"/> exposing the lifecycle actions stream and <c>ToTask()</c>.
	/// A superseded run observes cancellation; its rejection is suppressed from the group.
	/// </returns>
	public ThunkResult DispatchThunkLatest<TApi>(string cancellationGroup, ICallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var group = GetOrCreateThunkGroup(cancellationGroup);
		var cancellation = group.Begin(cancellationToken);
		return new(StartThunk(thunk, cancellation.Token, group, cancellation), cancellation);
	}

	/// <summary>
	/// Runs a thunk function in a cancellation group: starting a new thunk in the same group
	/// cancels the previous one (search-as-you-type semantics).
	/// </summary>
	/// <typeparam name="TApi">The thunk API type to run against.</typeparam>
	/// <typeparam name="TResult">The thunk's return type.</typeparam>
	/// <param name="cancellationGroup">Group name; latest dispatch per group wins.</param>
	/// <param name="thunk">The callable thunk function (typically <c>SomeThunk.Bind(...)</c>).</param>
	/// <param name="cancellationToken">Optional cancellation combined with the group's.</param>
	/// <returns>
	/// A <see cref="ThunkResult{TResult}"/> whose <c>ToTask()</c> yields the thunk's value.
	/// A superseded run observes cancellation; its rejection is suppressed from the group.
	/// </returns>
	public ThunkResult<TResult> DispatchThunkLatest<TApi, TResult>(string cancellationGroup, ICallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var group = GetOrCreateThunkGroup(cancellationGroup);
		var cancellation = group.Begin(cancellationToken);
		return new(StartThunk(thunk, cancellation.Token, group, cancellation), cancellation);
	}
}
