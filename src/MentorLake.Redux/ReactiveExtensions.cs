using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MentorLake.Redux.Effects;

public static class ReactiveExtensions
{
	public static IObservable<(T1 Item1, T2 Item2, T3 Item3)> WithLatestFrom<T1, T2, T3>(this IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3)
	{
		return o1.WithLatestFrom(o2).WithLatestFrom(o3).Select(t => (t.First.First, t.First.Second, t.Second));
	}

	public static IObservable<(T1 Item1, T2 Item2, T3 Item3, T4 Item4)> WithLatestFrom<T1, T2, T3, T4>(this IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4)
	{
		return o1.WithLatestFrom(o2, o3).WithLatestFrom(o4).Select(t => (t.First.Item1, t.First.Item2, t.First.Item3, t.Second));
	}

	/// <summary>
	/// Transforms a stream of keyed collections into one live stream per key: a new group
	/// is announced when a key first appears, existing groups receive updates as the key's
	/// value changes, and a group completes when the key disappears from the collection.
	/// This is the Rx/Redux lifecycle bridge that <c>GroupBy</c> cannot provide: Rx groups
	/// never close because operators treat every item as an occurrence, while collection
	/// snapshots are complete descriptions where absence means deletion.
	/// </summary>
	/// <typeparam name="TValue">The element type.</typeparam>
	/// <typeparam name="TKey">The key type, derived from each element by <paramref name="keySelector"/>.</typeparam>
	/// <param name="source">
	/// A stream of snapshots. Each emission is interpreted as the <em>complete</em> current
	/// set: keys missing from an emission are treated as removed and their groups complete.
	/// </param>
	/// <param name="keySelector">Extracts the identity of an element.</param>
	/// <returns>
	/// A stream that announces one <see cref="IGroupedObservable{TKey,TValue}"/> per key when
	/// the key first appears. <c>Key</c> is the value the key first appeared with; the key
	/// itself is <c>keySelector(group.Key)</c>. Each group emits <c>(Value, Index)</c> pairs,
	/// where <c>Index</c> is the element's position in the latest snapshot.
	/// </returns>
	/// <remarks>
	/// <para>
	/// Subscribe to a group <em>inside</em> the outer <c>OnNext</c> handler, synchronously.
	/// Groups are hot from the moment they are announced, and the first value is pushed
	/// immediately after the announcement returns, so late subscribers miss it.
	/// </para>
	/// <para>
	/// A key that is removed and later re-added is announced again as a new group; group
	/// identity is lifecycle-bound, not key-bound.
	/// </para>
	/// <para>
	/// Like all <see cref="Subject{T}"/>-based operators, <c>PerKey</c> expects
	/// <c>OnNext</c> calls not to overlap; store state notifications already satisfy this.
	/// </para>
	/// <para>
	/// Disposing the returned subscription detaches the source and completes every live
	/// group, so per-key pipelines tear down cleanly. Source completion or failure is
	/// mirrored to all live groups.
	/// </para>
	/// <para>See docs/per-key-streams.md for examples and the <c>GroupBy</c> comparison.</para>
	/// </remarks>
	public static IObservable<IGroupedObservable<TValue, (TValue, int)>> PerKey<TValue, TKey>(
		this IObservable<IEnumerable<TValue>> source, Func<TValue, TKey> keySelector) where TKey : IEquatable<TKey>
	{
		return Observable.Create<IGroupedObservable<TValue, (TValue, int)>>(obs =>
		{
			var groupSubjects = new Dictionary<TKey, Subject<(TValue, int)>>();
			var terminated = false;
			var sourceSubscription = source.Subscribe(elements =>
			{
				var elementList = new List<TValue>();
				elementList.AddRange(elements);

				for (var i = 0; i < elementList.Count; i++)
				{
					var e = elementList[i];
					var key = keySelector(e);

					if (!groupSubjects.ContainsKey(key))
					{
						groupSubjects.Add(key, new());
						obs.OnNext(new GroupedObservable<TValue, (TValue, int)>(e, groupSubjects[key]));
					}

					groupSubjects[key].OnNext((e, i));
				}

				var removedKeys = groupSubjects.Keys
					.Where(key => elements.All(e => !keySelector(e).Equals(key)))
					.ToList();

				foreach (var key in removedKeys)
				{
					groupSubjects[key].OnCompleted();
					groupSubjects.Remove(key);
				}
			},
			e =>
			{
				terminated = true;
				obs.OnError(e);
				foreach (var s in groupSubjects.Values) s.OnError(e);
				groupSubjects.Clear();
			},
			() =>
			{
				terminated = true;
				foreach (var s in groupSubjects.Values) s.OnCompleted();
				obs.OnCompleted();
				groupSubjects.Clear();
			});

			return new CompositeDisposable(
				sourceSubscription,
				Disposable.Create(() =>
				{
					if (terminated) return;
					foreach (var s in groupSubjects.Values) s.OnCompleted();
					groupSubjects.Clear();
				}));
		});
	}
}

file sealed class GroupedObservable<TKey, TValue> : ObservableBase<TValue>, IGroupedObservable<TKey, TValue>
{
	private readonly IObservable<TValue> _subject;

	public GroupedObservable(TKey key, IObservable<TValue> subject)
	{
		Key = key;
		_subject = subject;
	}

	public TKey Key { get; }

	protected override IDisposable SubscribeCore(IObserver<TValue> observer)
	{
		return _subject.Subscribe(observer);
	}
}
