using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Effects;

namespace MentorLake.Redux.Tests;

public record Row(string Id, string Name);

[TestClass]
public class PerKeyTests
{
	private Subject<IEnumerable<Row>> _source;
	private IObservable<IGroupedObservable<Row, (Row, int)>> _perKey;

	[TestInitialize]
	public void Initialize()
	{
		_source = new Subject<IEnumerable<Row>>();
		_perKey = _source.PerKey(r => r.Id);
	}

	private static Row R(string id, string name = "n") => new(id, name);

	private static IEnumerable<Row> Rows(params Row[] rows) => rows;

	private sealed class GroupHarness : IDisposable
	{
		public List<IGroupedObservable<Row, (Row, int)>> Announced { get; } = new();
		public Dictionary<string, List<(Row, int)>> Values { get; } = new();
		public HashSet<string> Completed { get; } = new();
		public List<Exception> Errors { get; } = new();
		public bool OuterCompleted;
		public Exception OuterError;
		private readonly List<IDisposable> _subs = new();
		private readonly IDisposable _outer;

		public GroupHarness(IObservable<IGroupedObservable<Row, (Row, int)>> source, bool subscribeToGroups = true)
		{
			_outer = source.Subscribe(
				new ObserverSink<IGroupedObservable<Row, (Row, int)>>(
					g =>
					{
						Announced.Add(g);
						if (!subscribeToGroups) return;
						var key = g.Key.Id;
						Values[key] = [];
						_subs.Add(g.Subscribe(new ObserverSink<(Row, int)>(
							v => Values[key].Add(v),
							ex => Errors.Add(ex),
							() => Completed.Add(key))));
					},
					ex => OuterError = ex,
					() => OuterCompleted = true));
		}

		public List<(Row, int)> This(string key) => Values[key];
		public int ThisCount(string key) => Values.TryGetValue(key, out var l) ? l.Count : 0;

		public void Dispose()
		{
			foreach (var s in _subs) s.Dispose();
			_outer.Dispose();
		}
	}

	[TestMethod]
	public void FirstEmission_AnnouncesOneGroupPerKey()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a"), R("b")));

		Assert.AreEqual(2, h.Announced.Count);
		Assert.AreEqual("a", h.Announced[0].Key.Id);
		Assert.AreEqual("b", h.Announced[1].Key.Id);
	}

	[TestMethod]
	public void GroupReceivesValueOnAnnouncement()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a", "Alice")));

		Assert.AreEqual("Alice", h.This("a")[0].Item1.Name);
		Assert.AreEqual(0, h.This("a")[0].Item2);
	}

	[TestMethod]
	public void ExistingKey_UpdatesItsGroupWithoutReAnnounce()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a", "v1")));
		_source.OnNext(Rows(R("a", "v2")));

		Assert.AreEqual(1, h.Announced.Count);
		Assert.AreEqual(2, h.ThisCount("a"));
		Assert.AreEqual("v2", h.This("a")[1].Item1.Name);
	}

	[TestMethod]
	public void IndexReflectsPositionInLatestEmission()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a"), R("b")));
		_source.OnNext(Rows(R("b"), R("a")));

		Assert.AreEqual(0, h.This("a")[0].Item2);
		Assert.AreEqual(1, h.This("b")[0].Item2);
		Assert.AreEqual(1, h.This("a")[1].Item2);
		Assert.AreEqual(0, h.This("b")[1].Item2);
	}

	[TestMethod]
	public void RemovedKey_CompletesItsGroupOnly()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a"), R("b")));
		_source.OnNext(Rows(R("a")));

		Assert.IsTrue(h.Completed.Contains("b"));
		Assert.IsFalse(h.Completed.Contains("a"));
	}

	[TestMethod]
	public void RemovedThenReAddedKey_IsAnnouncedAgain()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a")));
		_source.OnNext(Rows());
		_source.OnNext(Rows(R("a", "reborn")));

		Assert.AreEqual(2, h.Announced.Count(g => g.Key.Id == "a"));
	}

	[TestMethod]
	public void NewKey_AppearsMidStream()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a")));
		_source.OnNext(Rows(R("a"), R("b")));

		Assert.AreEqual(2, h.Announced.Count);
		Assert.AreEqual(1, h.ThisCount("b"));
	}

	[TestMethod]
	public void SourceCompletion_CompletesOuterAndAllGroups()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a"), R("b")));
		_source.OnCompleted();

		Assert.IsTrue(h.OuterCompleted);
		Assert.IsTrue(h.Completed.Contains("a"));
		Assert.IsTrue(h.Completed.Contains("b"));
	}

	[TestMethod]
	public void SourceError_FaultsOuterAndGroups()
	{
		var boom = new InvalidOperationException("boom");
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a")));
		_source.OnError(boom);

		Assert.AreSame(boom, h.OuterError);
		Assert.AreEqual(1, h.Errors.Count);
		Assert.AreSame(boom, h.Errors[0]);
	}

	[TestMethod]
	public void EmptyEmission_CompletesAllGroups()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a"), R("b")));
		_source.OnNext(Rows());

		Assert.IsTrue(h.Completed.Contains("a"));
		Assert.IsTrue(h.Completed.Contains("b"));
		Assert.IsFalse(h.OuterCompleted);
	}

	[TestMethod]
	public void DuplicateKeysInOneEmission_BothUpdateSameGroup()
	{
		using var h = new GroupHarness(_perKey);
		_source.OnNext(Rows(R("a", "v1"), R("a", "v2")));

		Assert.AreEqual(1, h.Announced.Count);
		Assert.AreEqual(2, h.ThisCount("a"));
	}

	[TestMethod]
	public void Bug7_DisposingOuterSubscription_LeavesGroupsAlive()
	{
		var completed = new HashSet<string>();
		var announced = new List<IGroupedObservable<Row, (Row, int)>>();

		var outer = _perKey.Subscribe(
			new ObserverSink<IGroupedObservable<Row, (Row, int)>>(
				g =>
				{
					announced.Add(g);
					g.Subscribe(new ObserverSink<(Row, int)>(_ => { }, _ => { }, () => completed.Add(g.Key.Id)));
				}));

		_source.OnNext(Rows(R("a")));
		outer.Dispose();

		_source.OnNext(Rows(R("b")));

		Assert.AreEqual(1, announced.Count, "source is detached after outer dispose");
		Assert.IsTrue(completed.Contains("a"),
			"KNOWN LEAK: group 'a' is never completed when the outer subscription is disposed");
	}
}

sealed class ObserverSink<T> : IObserver<T>
{
	private readonly Action<T> _next;
	private readonly Action<Exception> _error;
	private readonly Action _completed;

	public ObserverSink(Action<T> next = null, Action<Exception> error = null, Action completed = null)
	{
		_next = next;
		_error = error;
		_completed = completed;
	}

	public void OnCompleted() => _completed?.Invoke();
	public void OnError(Exception error) => _error?.Invoke(error);
	public void OnNext(T value) => _next?.Invoke(value);
}
