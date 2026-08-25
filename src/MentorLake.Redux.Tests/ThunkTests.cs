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

		var thunk = new ThunkAction<ThunkApi, int, string>("MultiArg", (api, a, b) =>
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
		var thunk = new ThunkFunc<ThunkApi, int, string, string>("Join", (api, a, b) =>
			Task.FromResult($"{a}:{b}"));

		var result = await _store.DispatchThunk(thunk.Bind(1, "two")).ToTask();
		Assert.AreEqual("1:two", result);
	}

	[TestMethod]
	public async Task LifecycleActions_UseExpectedNames()
	{
		var thunk = new ThunkAction<ThunkApi>("NamedJob", _ => Task.CompletedTask);
		var thunkDispatch = _store.DispatchThunk(thunk.Bind());
		var actions = thunkDispatch.Actions.Publish();
		var pending = actions.OfType<ThunkPending>().Take(1).ToTask();
		var fulfilled = actions.OfType<ThunkFulfilled>().Take(1).ToTask();
		actions.Connect();
		Task.WaitAll([pending, fulfilled], 1000);

		Assert.AreEqual("NamedJob/pending", pending.Result.ActionName);
		Assert.AreEqual("NamedJob/fulfilled", fulfilled.Result.ActionName);
	}

	[TestMethod]
	public async Task RejectedAction_UsesExpectedNameAndException()
	{
		var ex = new InvalidOperationException("boom");
		var thunk = new ThunkAction<ThunkApi>("FailJob", _ => throw ex);
		var result = _store.DispatchThunk(thunk.Bind());

		var rejected = await result.Actions.OfType<ThunkRejected>().Take(1).ToTask();
		Assert.AreEqual("FailJob/rejected", rejected.ActionName);
		Assert.AreSame(ex, rejected.Exception);
	}

	[TestMethod]
	[ExpectedException(typeof(TestException))]
	public async Task ThunkFunc_ToTask_RethrowsRejectedException()
	{
		var thunk = new ThunkFunc<ThunkApi, string>("FailFunc", _ => throw new TestException());
		await _store.DispatchThunk(thunk.Bind()).ToTask();
	}

	[TestMethod]
	public async Task NestedThunkRejection_SurfacesWhenAwaited()
	{
		var nested = new ThunkAction<ThunkApi>("NestedFail", _ => throw new TestException());
		var parent = new ThunkAction<ThunkApi>("Parent", async api =>
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
		var nested = new ThunkFunc<ThunkApi, string>("NestedOk", _ => Task.FromResult("child"));
		var parent = new ThunkFunc<ThunkApi, string>("ParentOk", async api =>
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

		var thunk = new ThunkAction<ThunkApi>("StoreVisible", _ => Task.CompletedTask);
		await _store.DispatchThunk(thunk.Bind()).ToTask();

		var actions = await storeActionsTask;
		Assert.AreEqual("StoreVisible/pending", actions[0].ActionName);
		Assert.AreEqual("StoreVisible/fulfilled", actions[1].ActionName);
	}

	[TestMethod]
	public async Task ThunkCanDispatchRegularActions()
	{
		var thunk = new ThunkAction<ThunkApi>("DispatchRegular", api =>
		{
			api.Dispatch(new UpdateFirstNameAction("FromThunk"));
			return Task.CompletedTask;
		});

		await _store.DispatchThunk(thunk.Bind()).ToTask();
		Assert.AreEqual("FromThunk", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task ThunkAction_CanBeCreatedWithoutName()
	{
		var ran = false;
		var thunk = new ThunkAction<ThunkApi>(_ =>
		{
			ran = true;
			return Task.CompletedTask;
		});

		var thunkDispatch = _store.DispatchThunk(thunk.Bind());
		var actions = thunkDispatch.Actions.Publish();
		var pending = actions.OfType<ThunkPending>().Take(1).ToTask();
		var fulfilled = actions.OfType<ThunkFulfilled>().Take(1).ToTask();
		actions.Connect();
		Task.WaitAll([pending, fulfilled], 1000);

		Assert.IsTrue(ran);
		Assert.AreEqual("/pending", pending.Result.ActionName);
		Assert.AreEqual("/fulfilled", fulfilled.Result.ActionName);
	}

	[TestMethod]
	public async Task ThunkFunc_CanBeCreatedWithoutName()
	{
		var thunk = new ThunkFunc<ThunkApi, string>(_ => Task.FromResult("ok"));

		var result = await _store.DispatchThunk(thunk.Bind()).ToTask();
		Assert.AreEqual("ok", result);
	}

	[TestMethod]
	public async Task MultiArgThunkAction_CanBeCreatedWithoutName()
	{
		int? captured = null;
		var thunk = new ThunkAction<ThunkApi, int>((_, value) =>
		{
			captured = value;
			return Task.CompletedTask;
		});

		await _store.DispatchThunk(thunk.Bind(42)).ToTask();
		Assert.AreEqual(42, captured);
	}

	[TestMethod]
	public async Task MultiArgThunkFunc_CanBeCreatedWithoutName()
	{
		var thunk = new ThunkFunc<ThunkApi, int, string, string>((_, a, b) => Task.FromResult($"{a}-{b}"));

		var result = await _store.DispatchThunk(thunk.Bind(3, "c")).ToTask();
		Assert.AreEqual("3-c", result);
	}

	[TestMethod]
	public async Task ThunkAction_ImplicitlyConvertsFromFunc()
	{
		Func<ThunkApi, Task> work = api =>
		{
			api.Dispatch(new UpdateFirstNameAction("Implicit"));
			return Task.CompletedTask;
		};

		ThunkAction<ThunkApi> thunk = work;
		await _store.DispatchThunk(thunk.Bind()).ToTask();
		Assert.AreEqual("Implicit", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task ThunkFunc_ImplicitlyConvertsFromFunc()
	{
		Func<ThunkApi, Task<int>> work = _ => Task.FromResult(99);

		ThunkFunc<ThunkApi, int> thunk = work;
		var result = await _store.DispatchThunk(thunk.Bind()).ToTask();
		Assert.AreEqual(99, result);
	}

	[TestMethod]
	public async Task MultiArgThunkAction_ImplicitlyConvertsFromFunc()
	{
		string captured = null;
		Func<ThunkApi, string, Task> work = (_, name) =>
		{
			captured = name;
			return Task.CompletedTask;
		};

		ThunkAction<ThunkApi, string> thunk = work;
		await _store.DispatchThunk(thunk.Bind("bound")).ToTask();
		Assert.AreEqual("bound", captured);
	}

	[TestMethod]
	public async Task MultiArgThunkFunc_ImplicitlyConvertsFromFunc()
	{
		Func<ThunkApi, int, int, Task<int>> work = (_, a, b) => Task.FromResult(a + b);

		ThunkFunc<ThunkApi, int, int, int> thunk = work;
		var result = await _store.DispatchThunk(thunk.Bind(4, 5)).ToTask();
		Assert.AreEqual(9, result);
	}

	[TestMethod]
	public async Task CallableThunkAction_ImplicitlyConvertsFromFunc()
	{
		Func<ThunkApi, Task> work = api =>
		{
			api.Dispatch(new UpdateFirstNameAction("CallableImplicit"));
			return Task.CompletedTask;
		};

		CallableThunkAction<ThunkApi> callable = work;
		await _store.DispatchThunk(callable).ToTask();
		Assert.AreEqual("CallableImplicit", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsLambdaDirectly()
	{
		await _store.DispatchThunk((ThunkApi api)=>
		{
			api.Dispatch(new UpdateFirstNameAction("DirectLambda"));
			return Task.CompletedTask;
		}).ToTask();

		Assert.AreEqual("DirectLambda", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsLambdaFuncDirectly()
	{
		var result = await _store.DispatchThunk((ThunkApi _) => Task.FromResult("from-lambda")).ToTask();
		Assert.AreEqual("from-lambda", result);
	}

	[TestMethod]
	public async Task ThunkApi_DispatchThunk_AcceptsLambdaDirectly()
	{
		var parent = new ThunkAction<ThunkApi>("ParentLambda", async api =>
		{
			await api.DispatchThunk(inner =>
			{
				inner.Dispatch(new UpdateFirstNameAction("NestedLambda"));
				return Task.CompletedTask;
			}).ToTask();
		});

		await _store.DispatchThunk(parent.Bind()).ToTask();
		Assert.AreEqual("NestedLambda", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task UnnamedThunk_RejectedActionName()
	{
		var ex = new InvalidOperationException("fail");
		var thunk = new ThunkAction<ThunkApi>(_ => throw ex);

		var rejected = await _store.DispatchThunk(thunk.Bind()).Actions.OfType<ThunkRejected>().Take(1).ToTask();
		Assert.AreEqual("/rejected", rejected.ActionName);
		Assert.AreSame(ex, rejected.Exception);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsNamedWrapperWithoutBind()
	{
		var thunk = new ThunkAction<ThunkApi>("NoBind", api =>
		{
			api.Dispatch(new UpdateFirstNameAction("NoBind"));
			return Task.CompletedTask;
		});

		var pending = await _store.DispatchThunk(thunk).Actions.OfType<ThunkPending>().Take(1).ToTask();
		Assert.AreEqual("NoBind/pending", pending.ActionName);
		Assert.AreEqual("NoBind", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsMultiArgNamedWrapperWithoutBind()
	{
		var thunk = new ThunkFunc<ThunkApi, int, string, string>("JoinNoBind", (_, a, b) => Task.FromResult($"{a}:{b}"));
		var result = await _store.DispatchThunk(thunk, 2, "b").ToTask();
		Assert.AreEqual("2:b", result);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsMethodGroupWithArgs()
	{
		await _store.DispatchThunk(UpdateFirstName, "MethodGroup").ToTask();
		Assert.AreEqual("MethodGroup", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsMethodGroupFuncWithArgs()
	{
		var result = await _store.DispatchThunk(JoinArgs, 3, "c").ToTask();
		Assert.AreEqual("3:c", result);
	}

	[TestMethod]
	public async Task DispatchThunk_AcceptsMultiArgLambdaDirectly()
	{
		int? capturedA = null;
		string capturedB = null;

		await _store.DispatchThunk((ThunkApi api, int? a, string b) =>
		{
			capturedA = a;
			capturedB = b;
			api.Dispatch(new UpdateFirstNameAction($"{a}-{b}"));
			return Task.CompletedTask;
		}, 7, "x").ToTask();

		Assert.AreEqual(7, capturedA);
		Assert.AreEqual("x", capturedB);
		Assert.AreEqual("7-x", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task ThunkApi_DispatchThunk_AcceptsMethodGroupWithArgs()
	{
		var parent = new ThunkAction<ThunkApi>("ParentArgs", async api =>
		{
			await api.DispatchThunk(UpdateFirstName, "NestedMethodGroup").ToTask();
			var joined = await api.DispatchThunk(JoinArgs, 1, "two").ToTask();
			Assert.AreEqual("1:two", joined);
		});

		await _store.DispatchThunk(parent.Bind()).ToTask();
		Assert.AreEqual("NestedMethodGroup", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	private static Task UpdateFirstName(ThunkApi api, string firstName)
	{
		api.Dispatch(new UpdateFirstNameAction(firstName));
		return Task.CompletedTask;
	}

	private static Task<string> JoinArgs(ThunkApi api, int a, string b) =>
		Task.FromResult($"{a}:{b}");
}
