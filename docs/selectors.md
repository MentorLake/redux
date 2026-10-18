# Selectors

Selectors are functions that read derived data out of the store. In MentorLake.Redux they are first-class, composable, and **memoized**: a selector recomputes only when one of its inputs actually changed, and subscribers are notified only when the selector's *output* actually changed.

- [Why Selectors](#why-selectors)
- [Feature Selectors](#feature-selectors)
- [Derived Selectors](#derived-selectors)
- [Composing Multiple Inputs](#composing-multiple-inputs)
- [Memoization](#memoization)
- [Previous-Result-Aware Projectors](#previous-result-aware-projectors)
- [Custom Equality](#custom-equality)
- [Reactive Selection](#reactive-selection)
- [Best Practices](#best-practices)

## Why Selectors

Reading state directly couples you to the shape of the store and forces every consumer to repeat the same projections and filtering. Selectors give you:

- **A query layer over state.** Consumers ask for `FullName`, not `GetFeatureState<PersonState>().FirstName + " " + ...`.
- **Performance.** Memoization means expensive projections (sorting, grouping, joining) run only when their inputs change — not on every state update.
- **Emission control.** Custom comparers let a subscriber ignore changes it doesn't care about (e.g., only the first name matters, not the whole person).
- **Testability.** A selector is a pure function of state; test it with a hand-built `StoreState`.

The core contract:

```csharp
public interface ISelector<out TOutput>
{
    TOutput Apply(StoreState input);                  // one-shot read
    IObservable<TOutput> Apply(IObservable<StoreState> input); // reactive stream
}
```

## Feature Selectors

`CreateFeature<T>` produces a selector for one feature slice:

```csharp
public static class MySelectors
{
    public static readonly ISelector<PersonState> Person = SelectorFactory.CreateFeature<PersonState>();
    public static readonly ISelector<AddressState> Address = SelectorFactory.CreateFeature<AddressState>();
}
```

## Derived Selectors

`SelectorFactory.Create` builds a selector from an upstream selector and a projection:

```csharp
public static readonly ISelector<string> FirstName =
    SelectorFactory.Create(MySelectors.Person, p => p.FirstName);

public static readonly ISelector<string> FullName =
    SelectorFactory.Create(MySelectors.Person,
        p => $"{p.FirstName} {p.LastName}");
```

You can also project directly from the whole `StoreState` for cross-cutting reads:

```csharp
public static readonly ISelector<bool> HasAddress =
    SelectorFactory.Create((StoreState s) => s.GetFeatureState<AddressState>().ZipCode.Length > 0);
```

## Composing Multiple Inputs

Selectors can combine up to **seven** upstream selectors:

```csharp
public static readonly ISelector<string> PersonSummary =
    SelectorFactory.Create(MySelectors.Person, MySelectors.Address,
        (person, address) => $"{person.FirstName} ({address.ZipCode})");

public static readonly ISelector<decimal> CartTotalWithTax =
    SelectorFactory.Create(CartItems, TaxRate, Discount,
        (items, rate, discount) => items.Sum(i => i.Price) * (1 + rate) - discount);
```

Composition is the normal way to build a selector graph: leaf selectors read feature state, intermediate selectors derive values, and "view selectors" produce exactly the shape a component needs.

## Memoization

Every selector created by `SelectorFactory` caches its result:

- A **derived selector** stores the last result of each of its input selectors. When `Apply` is called, it first evaluates its inputs; if every input result equals the cached one, it returns the cached output **without running the projection**.
- A **reactive selection** (`store.Select(selector)`) additionally applies `DistinctUntilChanged`, so subscribers only see output changes.

This makes it safe to hang expensive work off selectors:

```csharp
public static readonly ISelector<ImmutableList<Order>> RecentOrders =
    SelectorFactory.Create(AllOrders,
        orders => orders
            .Where(o => o.Date > DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(o => o.Date)
            .ToImmutableList());
```

`RecentOrders` re-sorts only when `AllOrders` returns a different list — dispatches that touch unrelated state cost nothing.

> **Share selector instances.** Memoization lives in the selector object, so selectors should be `static readonly` fields (or singletons). Creating a selector inside a method throws away the cache on every call and defeats the purpose.

## Previous-Result-Aware Projectors

For very large derived values, even recomputing on input change can be wasteful. `Create` overloads whose projection takes a *previous run* tuple let you reuse or incrementally update the prior result:

```csharp
// projection signature: (previousResult, previousInputs)?, currentInputs...
public static readonly ISelector<ImmutableDictionary<string, int>> WordCounts =
    SelectorFactory.Create(Documents,
        (previous, documents) =>
        {
            if (previous is null)
                return CountWords(documents);

            // previous.Value.Result is the last ImmutableDictionary,
            // previous.Value.Input1 is the last Documents value
            return IncrementalCount(previous.Value.Result, previous.Value.Input1, documents);
        });
```

The projection receives `null` for the previous tuple on the first run, and the previous result plus previous input values afterwards. Use this when the cost of a full recompute is proportional to data size and you can do better incrementally.

## Custom Equality

Change detection uses `EqualityComparer<T>.Default` unless you supply something else — either an `IEqualityComparer<T>` or a comparison function:

```csharp
private static bool CompareFirstNamesOnly(PersonState x, PersonState y)
{
    if (ReferenceEquals(x, y)) return true;
    if (x == null || y == null) return false;
    return x.FirstName.Equals(y.FirstName);
}

// Either form works:
var personByFirstName = SelectorFactory.Create(MySelectors.Person, p => p, CompareFirstNamesOnly);
var personByFirstName2 = MySelectors.Person.WithComparer(CompareFirstNamesOnly);
```

Subscribers to `personByFirstName` are notified only when the *first name* changes, even though the underlying `PersonState` may have changed in other ways.

For sequence outputs, `WithSequenceComparer` compares element-by-element with your function:

```csharp
public static readonly ISelector<ImmutableList<string>> Names =
    SelectorFactory.Create(MySelectors.Person, p => ImmutableList.Create(p.FirstName, p.LastName));

var namesLoose = Names.WithSequenceComparer((a, b) =>
    a.Equals(b, StringComparison.OrdinalIgnoreCase));
```

This is valuable when outputs are rebuilt on every recomputation (so reference equality always fails) but you only care about semantic changes.

## Reactive Selection

Two ways to observe derived state:

```csharp
// With a selector (recommended — memoized, composable)
store.Select(MySelectors.FullName)
    .Subscribe(name => Console.WriteLine(name));

// Ad-hoc projection over the whole state
store.Select(s => s.GetFeatureState<PersonState>().LastName)
    .Subscribe(Console.WriteLine);
```

Both emit the current value immediately upon subscription, then again only when the projected value changes. **The handler runs on the store's processor thread** — keep it trivial, or marshal off with `ObserveOn` first (see [Threading](threading.md)). If the projected value changes faster than you can handle it, pick a strategy from [Slow Consumers](slow-consumers.md) before subscribing. In components (Blazor, MAUI, WPF view models), subscribe and dispose with the component lifetime:

```csharp
IDisposable subscription = store.Select(MySelectors.FullName)
    .Subscribe(name => FullName = name);

// later, on teardown:
subscription.Dispose();
```

For one-shot reads, apply the selector to the current state:

```csharp
var fullName = MySelectors.FullName.Apply(store.State);
```

## Best Practices

1. **Define selectors as `static readonly` fields** in per-feature `*Selectors` classes so memoization is shared.
2. **Build small, then compose.** Prefer several leaf/intermediate selectors over one giant projection; memoization granularity follows the graph.
3. **Shape selectors to consumers.** Give a component one selector that returns exactly its view model; the memoizer keeps it cheap.
4. **Use custom comparers when consumers care about a subset** of the data, to suppress irrelevant re-renders.
5. **Keep projections pure.** A selector projection with side effects will run at unpredictable times (or not at all, when cached).

## See Also

- [Getting Started](getting-started.md) — where selectors fit in the loop
- [Threading](threading.md) — why selector handlers run on the processor thread
- [Slow Consumers](slow-consumers.md) — handling selector updates faster than you can process them
- [DataTable](datatable.md) — normalized data that pairs well with selectors
- [Testing](testing.md) — asserting memoization behavior
