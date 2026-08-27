using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux.Tests;

public class AsyncUserService
{
	public async Task<string> GetDisplayNameAsync(int id)
	{
		await Task.Delay(100);
		return $"user-{id}";
	}
}

public class CustomAsyncThunkApi(ThunkApiContext context, AsyncUserService users) : ThunkApi(context)
{
	public AsyncUserService Users { get; } = users;
}

[TestClass]
public class ThunkReducerTests
{
	private ReduxStore _store;

	[TestInitialize]
	public void Initialize()
	{
		_store = new ReduxStore();
	}

	[TestMethod]
	public async Task ThunkAction_DispatchedPendingAction_HasCorrectActionName()
	{
		var thunk = new ThunkAction<ThunkApi>("MyThunk", _ => Task.CompletedTask);
		var pending = await _store.DispatchThunk(thunk.Bind()).Actions.OfType<ThunkPending>().Take(1).ToTask();

		Assert.AreEqual("MyThunk/pending", pending.ActionName);
	}

	[TestMethod]
	public async Task ThunkAction_DispatchedFulfilledAction_HasCorrectActionName()
	{
		var thunk = new ThunkAction<ThunkApi>("MyThunk", _ => Task.CompletedTask);
		var fulfilled = await _store.DispatchThunk(thunk.Bind()).Actions.OfType<ThunkFulfilled>().Take(1).ToTask();

		Assert.AreEqual("MyThunk/fulfilled", fulfilled.ActionName);
	}

	[TestMethod]
	public async Task ThunkAction_DispatchedRejectedAction_HasCorrectActionName()
	{
		var thunk = new ThunkAction<ThunkApi>("MyThunk", _ => throw new InvalidOperationException("fail"));
		var rejected = await _store.DispatchThunk(thunk.Bind()).Actions.OfType<ThunkRejected>().Take(1).ToTask();

		Assert.AreEqual("MyThunk/rejected", rejected.ActionName);
	}

	[TestMethod]
	public async Task Reducer_ListeningToThunkFulfilled_OnlyFiresForMatchingThunkName()
	{
		_store.RegisterReducers(
			FeatureReducer.Build(new ThunkState())
				.On<ThunkFulfilled>("TargetThunk/fulfilled", (s, _) => new ThunkState { CallCount = s.CallCount + 1 }));

		var targetThunk = new ThunkAction<ThunkApi>("TargetThunk", _ => Task.CompletedTask);
		var otherThunk = new ThunkAction<ThunkApi>("OtherThunk", _ => Task.CompletedTask);

		await _store.DispatchThunk(otherThunk.Bind()).ToTask();
		await _store.DispatchThunk(targetThunk.Bind()).ToTask();

		Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task Reducer_ListeningToThunkFulfilled_DoesNotFireForUnrelatedThunk()
	{
		_store.RegisterReducers(
			FeatureReducer.Build(new ThunkState())
				.On<ThunkFulfilled>("SpecificThunk/fulfilled", (s, _) => new ThunkState { CallCount = s.CallCount + 1 }));

		var unrelatedThunk = new ThunkAction<ThunkApi>("UnrelatedThunk", _ => Task.CompletedTask);
		await _store.DispatchThunk(unrelatedThunk.Bind()).ToTask();

		Assert.AreEqual(0, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task Reducer_ListeningToThunkPending_OnlyFiresForMatchingThunkName()
	{
		_store.RegisterReducers(
			FeatureReducer.Build(new ThunkState())
				.On<ThunkPending>("TargetThunk/pending", (s, _) => new ThunkState { CallCount = s.CallCount + 1 }));

		var targetThunk = new ThunkAction<ThunkApi>("TargetThunk", _ => Task.CompletedTask);
		var otherThunk = new ThunkAction<ThunkApi>("OtherThunk", _ => Task.CompletedTask);

		await _store.DispatchThunk(otherThunk.Bind()).ToTask();
		await _store.DispatchThunk(targetThunk.Bind()).ToTask();

		Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task Reducer_ListeningToThunkFulfilled_SubstringThunkNameDoesNotCauseFalseMatch()
	{
		// "Thunk/fulfilled" should NOT match when action name is "LongerThunk/fulfilled"
		_store.RegisterReducers(
			FeatureReducer.Build(new ThunkState())
				.On<ThunkFulfilled>("Thunk/fulfilled", (s, _) => new ThunkState { CallCount = s.CallCount + 1 }));

		var longerNameThunk = new ThunkAction<ThunkApi>("LongerThunk", _ => Task.CompletedTask);
		await _store.DispatchThunk(longerNameThunk.Bind()).ToTask();

		Assert.AreEqual(0, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task TrulyAsyncThunk_DispatchesLifecycleActionsAndReducerFires()
	{
		_store.RegisterReducers(
			FeatureReducer.Build(new ThunkState())
				.On<ThunkFulfilled>("AsyncWork/fulfilled", (s, _) => new ThunkState { CallCount = s.CallCount + 1 }));

		var workCompleted = false;
		var thunk = new ThunkAction<ThunkApi>("AsyncWork", async _ =>
		{
			await Task.Delay(50);
			workCompleted = true;
		});

		var thunkDispatch = _store.DispatchThunk(thunk.Bind());
		var publishedThunkDispatch = thunkDispatch.Actions.Publish();
		var pendingTask = publishedThunkDispatch.OfType<ThunkPending>().Take(1).ToTask();
		var fulfilledTask = publishedThunkDispatch.OfType<ThunkFulfilled>().Take(1).ToTask();
		publishedThunkDispatch.Connect();

		Task.WaitAll([pendingTask, fulfilledTask], 1000);

		Assert.IsTrue(workCompleted);
		Assert.AreEqual("AsyncWork/pending", (await pendingTask).ActionName);
		Assert.AreEqual("AsyncWork/fulfilled", (await fulfilledTask).ActionName);
		Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task Reducer_ListeningToNamedThunk_DoesNotFireWhenUnnamedThunkDispatched()
	{
		_store.RegisterReducers(
			FeatureReducer.Build(new ThunkState())
				.On<ThunkFulfilled>("NamedThunk/fulfilled", (s, _) => new ThunkState { CallCount = s.CallCount + 1 }));

		var unnamedThunk = new ThunkAction<ThunkApi>(_ => Task.CompletedTask);
		await _store.DispatchThunk(unnamedThunk.Bind()).ToTask();

		Assert.AreEqual(0, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task TrulyAsyncThunk_WithCustomApi_DispatchesLifecycleActionsAndReducerFires()
	{
		var users = new AsyncUserService();
		var store = new ReduxStore();
		store.RegisterThunkApiFactory(ctx => new CustomAsyncThunkApi(ctx, users));
		store.RegisterReducers(
			FeatureReducer.Build(new PersonState("Hello", "World"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName }),
			FeatureReducer.Build(new ThunkState())
				.On<ThunkPending>("Test_LoadUserAsync/pending", (s, a) => new ThunkState { CallCount = s.CallCount + 1 })
				.On<ThunkPending>("/pending", (s, a) => new ThunkState { CallCount = s.CallCount + 1 }));

		var thunk = new ThunkAction<CustomAsyncThunkApi>("Test_LoadUserAsync", async api =>
		{
			var name = await api.Users.GetDisplayNameAsync(5);
			api.Dispatch(new UpdateFirstNameAction(name));
		});

		await store.DispatchThunk(thunk.Bind()).ToTask();

		Assert.AreEqual("user-5", store.State.GetFeatureState<PersonState>().FirstName);
		Assert.AreEqual(1, store.State.GetFeatureState<ThunkState>().CallCount);
	}
}
