using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux.Tests;

[TestClass]
public class ThunkTests
{
	private ReduxStore _store;

	[TestInitialize]
	public void Initialize()
	{
		_store = new ReduxStore();
		_store.RegisterReducers(
			FeatureReducer.Build(new PersonState("Hello", "World"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName }),
			FeatureReducer.Build(new ThunkState())
				.On<ThunkFulfilled>("MultiArg/fulfilled", (s, _) => new() { CallCount = s.CallCount + 1 }));
	}

	[TestMethod]
	public async Task MultiArgThunkAction_ReceivesAllArgs()
	{
		int? capturedA = null;
		string capturedB = null;

		var thunk = new ThunkAction<int, string>("MultiArg", (api, a, b) =>
		{
			capturedA = a;
			capturedB = b;
			return Task.CompletedTask;
		});

		await _store.DispatchThunk(thunk.Bind(7, "x")).ToTask();

		Assert.AreEqual(7, capturedA);
		Assert.AreEqual("x", capturedB);
		Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
	}

	[TestMethod]
	public async Task MultiArgThunkFunc_ReturnsProjectedResult()
	{
		var thunk = new ThunkFunc<int, string, string>("Join", (api, a, b) =>
			Task.FromResult($"{a}:{b}"));

		var result = await _store.DispatchThunk(thunk.Bind(1, "two")).ToTask();
		Assert.AreEqual("1:two", result);
	}

	[TestMethod]
	public async Task LifecycleActions_UseExpectedNames()
	{
		var thunk = new ThunkAction("NamedJob", _ => Task.CompletedTask);
		var result = _store.DispatchThunk(thunk.Bind());

		var pending = await result.Actions.OfType<ThunkPending>().Take(1).ToTask();
		var fulfilled = await result.Actions.OfType<ThunkFulfilled>().Take(1).ToTask();

		Assert.AreEqual("NamedJob/pending", pending.ActionName);
		Assert.AreEqual("NamedJob/fulfilled", fulfilled.ActionName);
	}

	[TestMethod]
	public async Task RejectedAction_UsesExpectedNameAndException()
	{
		var ex = new InvalidOperationException("boom");
		var thunk = new ThunkAction("FailJob", _ => throw ex);
		var result = _store.DispatchThunk(thunk.Bind());

		var rejected = await result.Actions.OfType<ThunkRejected>().Take(1).ToTask();
		Assert.AreEqual("FailJob/rejected", rejected.ActionName);
		Assert.AreSame(ex, rejected.Exception);
	}

	[TestMethod]
	[ExpectedException(typeof(TestException))]
	public async Task ThunkFunc_ToTask_RethrowsRejectedException()
	{
		var thunk = new ThunkFunc<string>("FailFunc", _ => throw new TestException());
		await _store.DispatchThunk(thunk.Bind()).ToTask();
	}

	[TestMethod]
	public async Task NestedThunkRejection_SurfacesWhenAwaited()
	{
		var nested = new ThunkAction("NestedFail", _ => throw new TestException());
		var parent = new ThunkAction("Parent", async api =>
		{
			await api.DispatchThunk(nested.Bind()).ToTask();
		});

		try
		{
			await _store.DispatchThunk(parent.Bind()).ToTask();
			Assert.Fail("Expected exception");
		}
		catch (TestException)
		{
			// expected from nested ToTask
		}
	}

	[TestMethod]
	public async Task NestedThunk_DoesNotCompleteParentEarly()
	{
		var nested = new ThunkFunc<string>("NestedOk", _ => Task.FromResult("child"));
		var parent = new ThunkFunc<string>("ParentOk", async api =>
		{
			var child = await api.DispatchThunk(nested.Bind()).ToTask();
			return $"parent:{child}";
		});

		var result = await _store.DispatchThunk(parent.Bind()).ToTask();
		Assert.AreEqual("parent:child", result);
	}

	[TestMethod]
	public async Task ThunkActions_AlsoReachStoreActions()
	{
		var storeActionsTask = _store.Actions
			.OfType<IAction>()
			.Where(a => a.ActionName.StartsWith("StoreVisible/"))
			.Take(2)
			.ToArray()
			.ToTask();

		var thunk = new ThunkAction("StoreVisible", _ => Task.CompletedTask);
		await _store.DispatchThunk(thunk.Bind()).ToTask();

		var actions = await storeActionsTask;
		Assert.AreEqual("StoreVisible/pending", actions[0].ActionName);
		Assert.AreEqual("StoreVisible/fulfilled", actions[1].ActionName);
	}

	[TestMethod]
	public async Task ThunkCanDispatchRegularActions()
	{
		var thunk = new ThunkAction("DispatchRegular", api =>
		{
			api.Dispatch(new UpdateFirstNameAction("FromThunk"));
			return Task.CompletedTask;
		});

		await _store.DispatchThunk(thunk.Bind()).ToTask();
		Assert.AreEqual("FromThunk", _store.State.GetFeatureState<PersonState>().FirstName);
	}
}
