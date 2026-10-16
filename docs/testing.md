# Testing

Redux architecture exists partly to make testing easy: reducers and selectors are pure functions, and thunks/effects have observable, deterministic behavior. This guide covers patterns for testing each piece, mirroring the library's own [test suite](../src/MentorLake.Redux.Tests).

The library's tests use MSTest with Microsoft.Testing.Platform; any framework (xUnit, NUnit, MSTest) works identically.

- [Running the Library's Tests](#running-the-librarys-tests)
- [Testing Reducers](#testing-reducers)
- [Testing Selectors](#testing-selectors)
- [Testing Thunks](#testing-thunks)
- [Testing Effects](#testing-effects)
- [Testing Integrations](#testing-integrations)
- [General Tips](#general-tips)

## Running the Library's Tests

```bash
dotnet test
```

## Testing Reducers

Reducers don't need the store — build a store with only the feature under test, dispatch, and assert on the resulting slice. This keeps tests honest (real dispatch semantics) with almost no setup:

```csharp
[TestClass]
public class PersonReducerTests
{
    private ReduxStore _store;

    [TestInitialize]
    public void Initialize()
    {
        _store = new ReduxStore();
        _store.RegisterReducers(
            FeatureReducer.Build(new PersonState("Hello", "World"))
                .On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName }));
    }

    [TestMethod]
    public void UpdateFirstName_ReplacesFirstName()
    {
        _store.Dispatch(new UpdateFirstNameAction("Bob"));

        var person = _store.State.GetFeatureState<PersonState>();
        Assert.AreEqual("Bob", person.FirstName);
        Assert.AreEqual("World", person.LastName); // untouched
    }
}
```

Test the important edge behaviors too:

```csharp
[TestMethod]
public void Dispatch_UnknownAction_DoesNotChangeFeatureState()
{
    var before = _store.State.GetFeatureState<PersonState>();
    _store.Dispatch(new UnknownAction());
    Assert.AreEqual(before, _store.State.GetFeatureState<PersonState>());
}

[TestMethod]
public void Dispatch_Null_IsNoOp()
{
    var before = _store.State;
    _store.Dispatch(null);
    Assert.AreSame(before, _store.State); // same reference — no churn
}
```

`Assert.AreSame` on `StoreState` is a useful idiom: it verifies a no-op dispatch didn't even allocate a new state container.

## Testing Selectors

Selectors are pure functions of state — apply them to `_store.State` directly:

```csharp
[TestMethod]
public void CreateFeature_ReadsFeatureState()
{
    var person = MySelectors.Person.Apply(_store.State);
    Assert.AreEqual("Hello", person.FirstName);
}

[TestMethod]
public void Create_FromSingleSelector_ProjectsValue()
{
    Assert.AreEqual("Hello", MySelectors.FirstName.Apply(_store.State));
}
```

**Memoization is observable.** Count projector invocations to prove caching works — this is exactly how the library tests it:

```csharp
[TestMethod]
public void MemoizedSelector_SkipsProjectorWhenInputsUnchanged()
{
    var projectCount = 0;
    var selector = SelectorFactory.Create(MySelectors.FirstName, name =>
    {
        projectCount++;
        return name.ToUpperInvariant();
    });

    Assert.AreEqual("HELLO", selector.Apply(_store.State));
    Assert.AreEqual("HELLO", selector.Apply(_store.State));
    Assert.AreEqual(1, projectCount);              // second Apply was cached

    _store.Dispatch(new UpdateFirstNameAction("Bob"));
    Assert.AreEqual("BOB", selector.Apply(_store.State));
    Assert.AreEqual(2, projectCount);              // input changed → recomputed
}
```

Test custom comparers by counting *emissions* over the reactive form:

```csharp
[TestMethod]
public void WithComparer_SuppressesIrrelevantEmissions()
{
    var emissions = new List<PersonState>();
    var selector = MySelectors.Person.WithComparer((x, y) => x.FirstName == y.FirstName);

    using var sub = _store.Select(selector).Subscribe(emissions.Add);

    _store.Dispatch(new UpdateLastNameAction("Changed")); // same first name
    _store.Dispatch(new UpdateFirstNameAction("Bob"));    // different first name

    Assert.AreEqual(2, emissions.Count); // initial + first-name change only
}
```

## Testing Thunks

**Await the result.** `ToTask()` gives deterministic completion — no sleeps, no polling:

```csharp
[TestMethod]
public async Task MultiArgThunkFunc_ReturnsProjectedResult()
{
    var thunk = new ThunkFunc<ThunkApi, int, string, string>("Join",
        (api, a, b) => Task.FromResult($"{a}:{b}"));

    var result = await _store.DispatchThunk(thunk.Bind(1, "two")).ToTask();
    Assert.AreEqual("1:two", result);
}
```

**Assert lifecycle actions.** Subscribe to the run's `Actions` stream *before* completion; `Publish()` + `ToTask()` makes each lifecycle step awaitable:

```csharp
[TestMethod]
public async Task LifecycleActions_UseExpectedNames()
{
    var thunk = new ThunkAction<ThunkApi>("NamedJob", _ => Task.CompletedTask);
    var actions = _store.DispatchThunk(thunk.Bind()).Actions.Publish();

    var pending = actions.OfType<ThunkPending>().Take(1).ToTask();
    var fulfilled = actions.OfType<ThunkFulfilled>().Take(1).ToTask();
    actions.Connect();

    await pending;
    var done = await fulfilled;
    Assert.AreEqual("NamedJob/fulfilled", done.ActionName);
}
```

**Assert rejections.** A throwing thunk rejects, and `ToTask()` rethrows the original exception:

```csharp
[TestMethod]
public async Task ThunkThatThrows_EmitsRejectedAndRethrows()
{
    var thunk = new ThunkAction<ThunkApi>("Boom", _ => throw new TestException());

    await Assert.ThrowsExceptionAsync<TestException>(
        async () => await _store.DispatchThunk(thunk.Bind()).ToTask());
}
```

**Assert reducer integration over lifecycle actions.** Register a slice that counts fulfillments, then dispatch the thunk:

```csharp
_store.RegisterReducers(
    FeatureReducer.Build(new ThunkState())
        .On<ThunkFulfilled>("MultiArg/fulfilled", (s, _) => new() { CallCount = s.CallCount + 1 }));

await _store.DispatchThunk(thunk.Bind(7, "x")).ToTask();

Assert.AreEqual(1, _store.State.GetFeatureState<ThunkState>().CallCount);
```

**Thunks with a custom API** are the easiest unit to test: construct the API with fakes and run the thunk work directly — no store needed if you only test logic:

```csharp
public class FakeUserService : IUserService { /* ... */ }

[TestMethod]
public async Task LoadUser_DispatchesLoadedAction()
{
    var dispatched = new List<object>();
    var api = new FakeAppThunkApi(new FakeUserService(), dispatched.Add);

    await AppThunks.LoadUser(api);

    Assert.IsInstanceOfType(dispatched.Single(), typeof(UserLoadedAction));
}
```

(For full lifecycle behavior, run through a real store with `RegisterThunkApiFactory`.)

## Testing Effects

Effects are functions from an action stream to an observable, so test them with a `Subject` as a fake action stream — no store required:

```csharp
[TestMethod]
public async Task SaveEffect_CallsServiceAndEmitsComplete()
{
    var service = new MockPersonService();
    var effect = new MyEffects(service).Create().Single(e => /* identify */ true);

    var actions = new Subject<object>();
    using var sub = effect.Run(actions).Subscribe();

    actions.OnNext(new SavePersonAction(new PersonState("Bob", "Smith")));

    await Retry(() => Assert.IsTrue(service.WasSaved)); // or await a completion signal
}
```

Because effects use `Concat`/`Switch` for concurrency, prefer asserting on the *service interactions* (what was called, how many times) rather than timing. For debounce/throttle effects, use Rx's `TestScheduler` for virtual time:

```csharp
var scheduler = new TestScheduler();
// .Throttle(TimeSpan.FromMilliseconds(300), scheduler) in the effect under test
scheduler.AdvanceBy(TimeSpan.FromMilliseconds(301).Ticks);
```

In practice, keep effect logic thin: the pipeline should delegate to a service, and the service carries the logic worth unit-testing.

## Testing Integrations

A full-loop test wires reducers + effects + thunks on one store and drives it like an application:

```csharp
[TestMethod]
public async Task SavePerson_EndToEnd()
{
    var store = new ReduxStore();
    store.RegisterReducers(new MyReducers());
    store.RegisterEffects(new MyEffects(new FakePersonService()));

    store.Dispatch(new SavePersonAction(new PersonState("Bob", "Smith")));

    await store.Actions.OfType<SavePersonCompleteAction>().FirstAsync().ToTask();

    Assert.IsTrue(store.State.GetFeatureState<SaveState>().Saved);
}
```

The pattern — dispatch a trigger, await an observable outcome, assert on state — replaces all `Task.Delay`-style flakiness.

## General Tips

1. **Never sleep.** Await lifecycle actions, `ToTask()`, or `FirstAsync().ToTask()` instead of delays.
2. **Fresh store per test** (`[TestInitialize]`) — selectors memoize, so shared stores leak state between tests.
3. **Beware shared static selectors in tests.** Their caches persist across tests; either build local selectors per test or account for cached results.
4. **Assert reference stability** (`AreSame`) when a test's point is "nothing changed".
5. **Test the stream, not the implementation** for effects: given these actions, this service should see these calls.

## See Also

- [Reducers](reducers.md), [Selectors](selectors.md), [Thunks](thunks.md), [Effects](effects.md)
