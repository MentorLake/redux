using System.Reactive.Linq;
using System.Reactive.Subjects;
using MentorLake.Redux.Thunks;

namespace MentorLake.Redux.Tests;

[TestClass]
public class ActionExtensionsTests
{
	[TestMethod]
	public void Action_FiltersByActionName()
	{
		var subject = new Subject<object>();
		IAction received = null;
		using var _ = subject.Action("custom/pending").Subscribe(a => received = a);

		subject.OnNext(new ThunkPending("other"));
		Assert.IsNull(received);

		var expected = new ThunkPending("custom");
		subject.OnNext(expected);
		Assert.AreSame(expected, received);
	}

	[TestMethod]
	public void Action_Generic_FiltersByTypeAndName()
	{
		var subject = new Subject<object>();
		ThunkFulfilled<string> received = null;
		using var _ = subject.Action<ThunkFulfilled<string>>("job/fulfilled").Subscribe(a => received = a);

		subject.OnNext(new ThunkFulfilled("job"));
		Assert.IsNull(received);

		subject.OnNext(new ThunkFulfilled<string>("other", "nope"));
		Assert.IsNull(received);

		var expected = new ThunkFulfilled<string>("job", "yes");
		subject.OnNext(expected);
		Assert.AreSame(expected, received);
		Assert.AreEqual("yes", received.Result);
	}

	[TestMethod]
	public void Action_IgnoresNonIActionObjects()
	{
		var subject = new Subject<object>();
		var count = 0;
		using var _ = subject.Action("anything").Subscribe(_ => count++);

		subject.OnNext(new object());
		subject.OnNext("string");
		Assert.AreEqual(0, count);
	}
}
