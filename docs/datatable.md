# DataTable

`DataTable<TKey, TValue>` is an immutable, normalized collection for entity-style state: items keyed by identity, with a stable ordered list of ids. It is the .NET analogue of the "entities by id" pattern from Redux Toolkit / NgRx.

- [Why Normalization](#why-normalization)
- [Basic Shape](#basic-shape)
- [IKeyed](#ikeyed)
- [Inserting and Updating](#inserting-and-updating)
- [Removing](#removing)
- [Reading](#reading)
- [Immutability Semantics](#immutability-semantics)
- [Patterns with Reducers and Selectors](#patterns-with-reducers-and-selectors)

## Why Normalization

Storing entities as a plain `List<T>` in state creates problems:

- Updating one entity means rebuilding (and reallocating) the whole list.
- Finding an entity is O(n).
- Duplicates can appear when data arrives from multiple sources.
- Order and membership get tangled with the entity data itself.

Normalization stores each entity **once**, keyed by id, and keeps ids in a separate ordered list. Updates touch one key; lookups are O(1); membership and order are explicit.

## Basic Shape

```csharp
public record DataTable<TKey, TValue>
{
    public ImmutableDictionary<TKey, TValue> ById { get; init; }
    public ImmutableList<TKey> AllIds { get; init; }
}
```

- `ById` — every entity, keyed by its id.
- `AllIds` — the ids in insertion order. Updating an existing entity does **not** reorder the list.

Both are `System.Collections.Immutable`, so every operation returns a new `DataTable` and leaves the previous one intact.

## IKeyed

Entities that carry their own id implement `IKeyed<TKey>`, which unlocks the value-based overloads:

```csharp
public interface IKeyed<TKey>
{
    public TKey Id { get; }
}

public record User(int Id, string Name, string Email) : IKeyed<int>;
```

## Inserting and Updating

`UpsertOne` inserts a new entity or replaces the existing one under the same key:

```csharp
var users = new DataTable<int, User>();

// Explicit key
users = users.UpsertOne(1, new User(1, "Ada", "ada@example.com"));

// Key taken from IKeyed<T>
users = users.UpsertOne(new User(2, "Bob", "bob@example.com"));

// Update in place (returns a new table; AllIds order preserved)
users = users.UpsertOne(new User(1, "Ada L.", "ada@example.com"));
```

`UpsertMany` batches inserts/updates from several common shapes:

```csharp
// IKeyed entities
users = users.UpsertMany(fetchedUsers);                    // IEnumerable<User>

// Key/value pairs
users = users.UpsertMany(response.UsersById);              // IEnumerable<KeyValuePair<int, User>>

// Tuples
users = users.UpsertMany(pairs.Select(u => (u.Id, u)));    // IEnumerable<(int, User)>
```

## Removing

```csharp
users = users.Remove(2);                                   // by key
users = users.Remove(someUser);                            // by IKeyed value
```

Removing a key that doesn't exist is a no-op (a new, equal table is returned).

## Reading

```csharp
User ada = users.Get(1);                                   // throws if missing
bool exists = users.ContainsKey(3);
IEnumerable<User> selected = users.GetMany(new[] { 1, 2 }); // throws if any key missing
```

For "try" semantics, check `ContainsKey` first or read `ById` directly:

```csharp
if (users.ById.TryGetValue(3, out var user)) { /* ... */ }
```

To render the full list in order, project `AllIds`:

```csharp
var ordered = users.AllIds.Select(id => users.ById[id]).ToImmutableList();
```

## Immutability Semantics

Every mutating operation returns a new `DataTable` via `with` expressions over immutable collections:

```csharp
public DataTable<TKey, TValue> UpsertOne(TKey key, TValue val) =>
    this with { ById = ById.SetItem(key, val), AllIds = ById.ContainsKey(key) ? AllIds : AllIds.Add(key) };
```

Consequences:

- **Reference equality is meaningful.** Two tables are the same instance only if nothing changed — exactly what selectors use to skip recomputation.
- **Old snapshots stay valid.** A selector holding yesterday's table still sees yesterday's data.
- **Never mutate `ById`/`AllIds` contents in place** — build new tables with the provided operations.

## Patterns with Reducers and Selectors

**State slice.** Put a table in a feature record:

```csharp
public record UsersState(DataTable<int, User> Users)
{
    public static UsersState Empty { get; } = new(new DataTable<int, User>());
}
```

**Reducers.** Map actions to table operations:

```csharp
FeatureReducer.Build(UsersState.Empty)
    .On<UsersLoadedAction>((state, action) => state with { Users = state.Users.UpsertMany(action.Users) })
    .On<UserSavedAction>((state, action) => state with { Users = state.Users.UpsertOne(action.User) })
    .On<UserDeletedAction>((state, action) => state with { Users = state.Users.Remove(action.UserId) })
```

**Selectors.** Derive views over the table; memoization keeps them cheap:

```csharp
public static class UsersSelectors
{
    public static readonly ISelector<UsersState> Users = SelectorFactory.CreateFeature<UsersState>();

    public static readonly ISelector<ImmutableList<User>> AllUsers =
        SelectorFactory.Create(Users, s => s.Users.AllIds.Select(id => s.Users.ById[id]).ToImmutableList());

    public static readonly ISelector<int> UserCount =
        SelectorFactory.Create(Users, s => s.Users.AllIds.Count);

    public static readonly ISelector<Func<int, User?>> UserById =
        SelectorFactory.Create(Users, s => (Func<int, User?>)(id =>
            s.Users.ById.TryGetValue(id, out var u) ? u : null));
}
```

Because `AllUsers` rebuilds a new list on each recomputation, give consumers an element comparer if they should ignore semantically identical refreshes:

```csharp
var stableUsers = UsersSelectors.AllUsers.WithSequenceComparer((a, b) => a.Equals(b));
```

**Relationships.** Normalization composes: keep `DataTable<int, Order>` and `DataTable<int, Customer>` in separate slices and join in selectors, rather than nesting objects and duplicating data.

## See Also

- [Reducers](reducers.md) — updating tables from actions
- [Selectors](selectors.md) — deriving views from tables

## See Also

- [Per-Key Streams](per-key-streams.md) — turn a table stream into one live stream per entity, closed on removal
- [Selectors](selectors.md) — memoized projections over tables
