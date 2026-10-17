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

	public void RegisterThunkApiFactory<TApi>(Func<ThunkApiContext, TApi> factory) where TApi : ThunkApi
	{
		ArgumentNullException.ThrowIfNull(factory);
		_thunkApiFactories[typeof(TApi)] = ctx => factory(ctx);
	}

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

	public ThunkResult DispatchThunk<TApi>(ICallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var owned = CreateOwnedCancellation(cancellationToken, out var token);
		return new(StartThunk(thunk, token, null, owned), owned);
	}

	public ThunkResult<TResult> DispatchThunk<TApi, TResult>(ICallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var owned = CreateOwnedCancellation(cancellationToken, out var token);
		return new(StartThunk(thunk, token, null, owned), owned);
	}

	public ThunkResult DispatchThunkLatest<TApi>(string cancellationGroup, ICallableThunkAction<TApi> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var group = GetOrCreateThunkGroup(cancellationGroup);
		var cancellation = group.Begin(cancellationToken);
		return new(StartThunk(thunk, cancellation.Token, group, cancellation), cancellation);
	}

	public ThunkResult<TResult> DispatchThunkLatest<TApi, TResult>(string cancellationGroup, ICallableThunkFunc<TApi, TResult> thunk, CancellationToken cancellationToken = default) where TApi : ThunkApi
	{
		var group = GetOrCreateThunkGroup(cancellationGroup);
		var cancellation = group.Begin(cancellationToken);
		return new(StartThunk(thunk, cancellation.Token, group, cancellation), cancellation);
	}
}
