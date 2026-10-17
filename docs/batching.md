# Batching

Each dispatch normally pushes a new state through the store's observable, notifying every selector and subscriber. When you dispatch several actions in a row (e.g. hydrating several features after a login response), that means several UI renders for one logical update. `Batch` coalesces them into a single state notification.

- [Usage](#usage)
- [Semantics](#semantics)
- [When to Use It](#when-to-use-it)

## Usage

```csharp
store.Batch(() =>
{
    store.Dispatch(new SetUserAction(user));
    store.Dispatch(new SetPreferencesAction(prefs));
    store.Dispatch(new SetCartAction(cart));
});
// subscribers are notified once, with the final state
```

`Batch` is intentionally synchronous. Batching holds a counter open for the duration of the body, so an `await` inside it could resume on a different thread and race that counter. Dispatch before and after the await instead, or wrap each synchronous burst in its own `Batch`.

## Semantics

- `store.State` is updated **synchronously** by every dispatch inside the batch — reducers always see the latest state.
- The action stream (`store.Actions`) still emits **every** action; only state notifications are coalesced.
- Nested batches join the outermost one; the single notification fires when the outermost batch completes.
- If the batch body throws, the pending notification is still flushed before the exception propagates, so subscribers never observe a stale state silently.
- A batch with no dispatches produces no notification.

## When to Use It

Use batching when one logical event produces multiple actions:

- Login/hydration: token + profile + feature flags
- Bulk edits: several entity updates from one server response
- Undo/redo restore: replaying a group of actions as one visual update

Without batching, each dispatch triggers a full selector re-evaluation pass — in Blazor or MAUI that is one render per action instead of one per logical update.
