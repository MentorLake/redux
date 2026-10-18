using System.Collections.Immutable;

namespace MentorLake.Redux;

/// <summary>
/// Implemented by values stored in a <see cref="DataTable{TKey, TValue}"/> so the table
/// can derive the key from the value itself.
/// </summary>
/// <typeparam name="TKey">The key type identifying the value.</typeparam>
public interface IKeyed<TKey>
{
	/// <summary>The value's identity within the table.</summary>
	public TKey Id { get; }
}
/// <summary>
/// Immutable normalized collection: values keyed by identity plus insertion-ordered ids.
/// Every operation returns a new table, so it works as feature state with reference-based
/// change detection.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type; implement <see cref="IKeyed{TKey}"/> for key-from-value overloads.</typeparam>
public record DataTable<TKey, TValue>
{
	/// <summary>Lookup by key.</summary>
	public ImmutableDictionary<TKey, TValue> ById { get; init; } = ImmutableDictionary<TKey, TValue>.Empty;
	/// <summary>Ids in insertion order.</summary>
	public ImmutableList<TKey> AllIds { get; init; } = ImmutableList<TKey>.Empty;

	/// <summary>Inserts or replaces the value for <paramref name="key"/>.</summary>
	public DataTable<TKey, TValue> UpsertOne(TKey key, TValue val)
	{
		return this with { ById = ById.SetItem(key, val), AllIds = ById.ContainsKey(key) ? AllIds : AllIds.Add(key) };
	}

	/// <summary>Inserts or replaces a value, deriving the key from <see cref="IKeyed{TKey}.Id"/>.</summary>
	public DataTable<TKey, TValue> UpsertOne<T>(T val) where T : IKeyed<TKey>, TValue
	{
		return this with { ById = ById.SetItem(val.Id, val), AllIds = ById.ContainsKey(val.Id) ? AllIds : AllIds.Add(val.Id) };
	}

	/// <summary>Inserts or replaces many values, deriving keys from <see cref="IKeyed{TKey}.Id"/>.</summary>
	public DataTable<TKey, TValue> UpsertMany<T>(IEnumerable<T> items) where T : IKeyed<TKey>, TValue
	{
		return items.Aggregate(this, (x, y) => x.UpsertOne(y));
	}

	/// <summary>Inserts or replaces many key/value pairs.</summary>
	public DataTable<TKey, TValue> UpsertMany(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return items.Aggregate(this, (x, y) => x.UpsertOne(y.Key, y.Value));
	}

	/// <summary>Inserts or replaces many key/value tuples.</summary>
	public DataTable<TKey, TValue> UpsertMany(IEnumerable<Tuple<TKey, TValue>> items)
	{
		return items.Aggregate(this, (x, y) => x.UpsertOne(y.Item1, y.Item2));
	}

	/// <summary>Inserts or replaces many key/value pairs.</summary>
	public DataTable<TKey, TValue> UpsertMany(IEnumerable<(TKey, TValue)> items)
	{
		return items.Aggregate(this, (x, y) => x.UpsertOne(y.Item1, y.Item2));
	}

	/// <summary>Removes the entry for <paramref name="key"/>.</summary>
	public DataTable<TKey, TValue> Remove(TKey key)
	{
		return this with { ById = ById.Remove(key), AllIds = AllIds.Remove(key) };
	}

	/// <summary>Removes a value by its <see cref="IKeyed{TKey}.Id"/>.</summary>
	public DataTable<TKey, TValue> Remove<T>(T val) where T : IKeyed<TKey>, TValue
	{
		return this with { ById = ById.Remove(val.Id), AllIds = AllIds.Remove(val.Id) };
	}

	/// <summary>Whether the table holds an entry for <paramref name="key"/>.</summary>
	public bool ContainsKey(TKey key)
	{
		return ById.ContainsKey(key);
	}

	/// <summary>Projects keys to their values, preserving the given order.</summary>
	/// <exception cref="KeyNotFoundException">A key is not in the table.</exception>
	public IEnumerable<TValue> GetMany(IEnumerable<TKey> keys)
	{
		foreach (var k in keys)
		{
			yield return ById[k];
		}
	}

	/// <summary>Looks up a value by key.</summary>
	/// <exception cref="KeyNotFoundException">The key is not in the table.</exception>
	public TValue Get(TKey key)
	{
		return ById[key];
	}

	public virtual bool Equals(DataTable<TKey, TValue> other) => ReferenceEquals(this, other);
}
