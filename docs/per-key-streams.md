# Per-Key Streams (`PerKey`)

Rendering a collection is the one place where Redux and Rx pull in opposite directions. `PerKey` resolves it: it turns a stream of keyed collections into **one live stream per item** — created when the item appears, updated when it changes, and **closed when it disappears**.

```csharp
store.Select(ItemsTableSelector)          // IObservable<IEnumerable<Item>>
     .PerKey(i => i.Id)
     .Subscribe(group =>
     {
         // once per key, when the key first appears
         group.Subscribe(item => RenderItem(group.Key, item));
         // when the item is removed, this stream completes —
         // per-item pipelines tear themselves down
     });
```

## The Gap It Fills

Redux and Rx model time differently:

- **Redux thinks in snapshots.** The state *is*. A `DataTable` emission is a complete description of the current set: a key that isn't in the snapshot isn't "quiet" — it's **gone**.
- **Rx thinks in events.** Streams carry happenings; there is no present tense, and the absence of an event means nothing.

Rx's closest operator is `GroupBy`. It demultiplexes by key beautifully — but its groups **never close**. The operator cannot know that a key's absence from the latest snapshot means deletion, because `IObservable<T>` deliberately carries no such convention. `GroupByUntil` closes groups on a timer, not on data.

| | `GroupBy` | `PerKey` |
|---|---|---|
| New key → new stream | yes | yes |
| Value change → that stream emits | yes | yes |
| Key removed → stream completes | **never** | yes |
| Understands snapshots | no — every item is an occurrence | yes — absence means deletion |
| Disposing the outer subscription | n/a | completes all live groups |

So the pattern every UI eventually needs — *one stream per row, with birth and death* — has no Rx primitive. Without `PerKey`, consumers hand-roll it: subscribe to snapshots, diff key sets, manage a dictionary of subscriptions, remember to tear them down. `PerKey` promotes that application code to a library operator, made sound by `DataTable`'s documented meaning ("this is the complete current set") — exactly the assumption the operator needs.

## Usage

### Basic: one stream per entity

```csharp
store.Select(s => s.GetFeatureState<PeopleState>().People)
     .PerKey(p => p.Id)
     .Subscribe(group =>
     {
         group.Subscribe(person => UpdateRow(group.Key, person));
     });
```

### Composed: per-item pipelines with other state

Because each group is an ordinary `IObservable<T>`, per-item pipelines compose with the full Rx toolbox — and complete automatically when the item goes away:

```csharp
store.Select(ItemsSelector)
     .PerKey(i => i.Id)
     .Subscribe(group =>
     {
         group
             .WithLatestFrom(store.Select(CurrencySelector), (item, currency) => Format(item, currency))
             .Subscribe(text => RenderRow(group.Key, text));
         // item removed → group completes → WithLatestFrom completes → row self-disposes
     });
```

Hoist ancillary selector streams (like `CurrencySelector` above) outside the handler so every item shares one subscription.

### Reordering

Each group emits `(Value, Index)` pairs, where `Index` is the item's position in the latest snapshot — useful for detecting reorderings without diffing the whole collection.

## Rules of Use

- **Subscribe to a group inside the outer `OnNext` handler, synchronously.** Groups are hot from the moment they are announced; the operator pushes the first value immediately after announcing, so late subscribers miss it. Build your per-item pipeline inside the handler.
- **A removed key that comes back is a new group.** Treat group identity as lifecycle-bound, not key-bound: the same key can be announced more than once over the life of the source.
- **Serialize emissions.** Like all `Subject`-based operators, `PerKey` expects `OnNext` calls not to overlap. Store state notifications already satisfy this.
- **Dispose completes everything.** Disposing the outer subscription detaches the source and completes every live group, so per-item pipelines unwind cleanly.
- **Empty snapshot closes everything.** An emission with no elements completes all groups — by design (absence means deletion), but be aware a transient empty state will close and later re-open every row.

## API

```csharp
public static IObservable<IGroupedObservable<TValue, (TValue, int)>> PerKey<TValue, TKey>(
    this IObservable<IEnumerable<TValue>> source,
    Func<TValue, TKey> keySelector)
    where TKey : IEquatable<TKey>;
```

`group.Key` is the *value* the key first appeared with (the `IGroupedObservable` shape); the key itself is `keySelector(group.Key)`.

## See Also

- [DataTable](datatable.md) — the normalized collection `PerKey` is designed to unbundle
- [Selectors](selectors.md) — building the table projections `PerKey` consumes
- [Reactive Programming](reactive-programming.md) — Rx fundamentals used in per-key pipelines
- [Blazor View Models](blazor-viewmodels.md) — per-row components driven by per-key streams
