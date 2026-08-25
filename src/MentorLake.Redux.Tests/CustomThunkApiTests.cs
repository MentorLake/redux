using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux.Tests;

[TestClass]
public class CustomThunkApiTests
{
	private ReduxStore _store;
	private FakeUserService _users;

	[TestInitialize]
	public void Initialize()
	{
		_users = new FakeUserService();
		_store = new ReduxStore();
		_store.RegisterReducers(
			FeatureReducer.Build(new PersonState("Hello", "World"))
				.On<UpdateFirstNameAction>((s, a) => s with { FirstName = a.FirstName }));
		_store.UseThunkApi(ctx => new AppThunkApi(ctx, _users));
	}

	[TestMethod]
	public async Task CustomThunkApi_LambdaReceivesDerivedType()
	{
		AppThunkApi captured = null;

		await _store.Using<AppThunkApi>().Dispatch(api =>
		{
			captured = api;
			Assert.AreSame(_users, api.Users);
			return Task.CompletedTask;
		}).ToTask();

		Assert.IsNotNull(captured);
		Assert.IsInstanceOfType(captured, typeof(AppThunkApi));
	}

	[TestMethod]
	public async Task CustomThunkApi_CanUseInjectedDependency()
	{
		await _store.Using<AppThunkApi>().Dispatch(async api =>
		{
			var name = await api.Users.GetDisplayNameAsync(7);
			api.Dispatch(new UpdateFirstNameAction(name));
		}).ToTask();

		Assert.AreEqual("user-7", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task CustomThunkApi_NamedViaDispatchNameArgument()
	{
		var result = _store.Using<AppThunkApi>().Dispatch("LoadUser", async api =>
		{
			var name = await api.Users.GetDisplayNameAsync(3);
			api.Dispatch(new UpdateFirstNameAction(name));
		});

		var pending = await result.Actions.OfType<ThunkPending>().Take(1).ToTask();
		Assert.AreEqual("LoadUser/pending", pending.ActionName);
		Assert.AreEqual("user-3", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task CustomThunkApi_NamedFuncWithArgs()
	{
		var result = await _store.Using<AppThunkApi>().Dispatch("JoinUser", async (api, id) =>
		{
			var name = await api.Users.GetDisplayNameAsync(id);
			return $"hello:{name}";
		}, 9).ToTask();

		Assert.AreEqual("hello:user-9", result);
	}

	[TestMethod]
	public async Task CustomThunkApi_DispatchWithArgs()
	{
		var result = await _store.Using<AppThunkApi>().Dispatch(async (api, id) =>
		{
			return await api.Users.GetDisplayNameAsync(id);
		}, 4).ToTask();

		Assert.AreEqual("user-4", result);
	}

	[TestMethod]
	public async Task CustomThunkApi_NestedDispatchViaUsing()
	{
		var result = await _store.Using<AppThunkApi>().Dispatch("Parent", async api =>
		{
			var child = await api.Using<AppThunkApi>().Dispatch(async inner =>
			{
				Assert.IsInstanceOfType(inner, typeof(AppThunkApi));
				return await inner.Users.GetDisplayNameAsync(1);
			}).ToTask();
			return $"parent:{child}";
		}).ToTask();

		Assert.AreEqual("parent:user-1", result);
	}

	[TestMethod]
	public async Task CustomThunkApi_MissingFactory_ThrowsHelpfulError()
	{
		var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _store.Using<SimpleThunkApi>().Dispatch(_ => Task.CompletedTask).ToTask());
		StringAssert.Contains(ex.Message, nameof(SimpleThunkApi));
	}

	[TestMethod]
	public async Task DefaultThunkApi_StillWorksAlongsideCustomFactory()
	{
		await _store.DispatchThunk(api =>
		{
			api.Dispatch(new UpdateFirstNameAction("DefaultPath"));
			return Task.CompletedTask;
		}).ToTask();

		Assert.AreEqual("DefaultPath", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task CustomThunkApi_DispatchMethodThunkWithNoArgs()
	{
		var result = await _store.Using<AppThunkApi>().Dispatch(MyThunks.Test).ToTask();
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task CustomThunkApi_DispatchMethodThunkAction()
	{
		await _store.Using<AppThunkApi>().Dispatch(MyThunks.LoadUser).ToTask();
		Assert.AreEqual("user-5", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task CustomThunkApi_DispatchMethodThunkWithArgs()
	{
		var result = await _store.Using<AppThunkApi>().Dispatch(MyThunks.GetDisplayName, 11).ToTask();
		Assert.AreEqual("user-11", result);
	}

	[TestMethod]
	public async Task CustomThunkApi_NestedMethodGroupDispatch()
	{
		var result = await _store.Using<AppThunkApi>().Dispatch(MyThunks.Nested).ToTask();
		Assert.AreEqual("nested:user-2", result);
	}

	[TestMethod]
	public async Task CustomThunkApi_NamedThunkActionWrapper_OnBaseSugar()
	{
		var thunk = new ThunkAction<ThunkApi>("BaseNamed", api =>
		{
			api.Dispatch(new UpdateFirstNameAction("from-wrapper"));
			return Task.CompletedTask;
		});

		var pending = await _store.DispatchThunk(thunk).Actions.OfType<ThunkPending>().Take(1).ToTask();
		Assert.AreEqual("BaseNamed/pending", pending.ActionName);
		Assert.AreEqual("from-wrapper", _store.State.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public async Task CustomThunkApi_CachedDispatcher()
	{
		var thunks = _store.Using<AppThunkApi>();
		var name = await thunks.Dispatch(MyThunks.GetDisplayName, 8).ToTask();
		Assert.AreEqual("user-8", name);
	}

	private sealed class FakeUserService
	{
		public Task<string> GetDisplayNameAsync(int id) => Task.FromResult($"user-{id}");
	}

	private sealed class AppThunkApi : ThunkApi
	{
		public AppThunkApi(ThunkApiContext context, FakeUserService users) : base(context)
		{
			Users = users;
		}

		public FakeUserService Users { get; }
	}

	private sealed class SimpleThunkApi : ThunkApi
	{
		public SimpleThunkApi(ThunkApiContext context) : base(context)
		{
			Marker = true;
		}

		public bool Marker { get; }
	}

	private static class MyThunks
	{
		public static Task<bool> Test(AppThunkApi api) => Task.FromResult(true);

		public static async Task LoadUser(AppThunkApi api)
		{
			var name = await api.Users.GetDisplayNameAsync(5);
			api.Dispatch(new UpdateFirstNameAction(name));
		}

		public static Task<string> GetDisplayName(AppThunkApi api, int id) =>
			api.Users.GetDisplayNameAsync(id);

		public static async Task<string> Nested(AppThunkApi api)
		{
			var name = await api.Using<AppThunkApi>().Dispatch(GetDisplayName, 2).ToTask();
			return $"nested:{name}";
		}
	}
}
