# Batching

Each dispatch normally pushes a new state through the store's observable, notifying every selector and subscriber. When several actions form one logical update (e.g. hydrating several features after a login response), that means several UI renders. Passing multiple actions to a single `Dispatch` coalesces them into one state notification.

- [Usage](#usage)
- [Semantics](#semantics)
- [When to Use It](#when-to-use-it)

## Usage

```csharp
store.Dispatch(
    new SetUserAction(user),
    new SetPreferencesAction(prefs),
    new SetCartAction(cart));
// subscribers are notified once, with the final state
```

The actions are submitted to the store as a single unit. The store processes them in order on its dispatch processor, then emits one state notification.

## Semantics

- The actions are applied **in order**, as one atomic unit. Subscribers never observe a partially applied batch.
- The action stream (`store.Actions`) still emits **every** action; only state notifications are coalesced.
- `store.State` reflects the batch only once the whole batch has been applied. `Dispatch` blocks until then, so reading state after the call always sees the final result.
- Batches from any number of threads are safe: each batch is a single item in the store's queue, so concurrent batches are applied whole, one after another.
- If an action's middleware or reducer throws, the exception propagates to the dispatching caller and the store continues with the next queued item.
- A batch that changes no state produces no notification.

## When to Use It

Use batching when one logical event produces multiple actions:

- Login/hydration: token + profile + feature flags
- Bulk edits: several entity updates from one server response
- Undo/redo restore: replaying a group of actions as one visual update

Without batching, each dispatch triggers a full selector re-evaluation pass — in Blazor or MAUI that is one render per action instead of one per logical update.
