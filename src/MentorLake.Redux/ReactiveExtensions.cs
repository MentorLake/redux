using System.Reactive.Linq;

namespace MentorLake.Redux.Effects;

internal static class ReactiveExtensions
{
	public static IObservable<(T1 Item1, T2 Item2, T3 Item3)> WithLatestFrom<T1, T2, T3>(this IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3)
	{
		return o1.WithLatestFrom(o2).WithLatestFrom(o3).Select(t => (t.First.First, t.First.Second, t.Second));
	}

	public static IObservable<(T1 Item1, T2 Item2, T3 Item3, T4 Item4)> WithLatestFrom<T1, T2, T3, T4>(this IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4)
	{
		return o1.WithLatestFrom(o2, o3).WithLatestFrom(o4).Select(t => (t.First.Item1, t.First.Item2, t.First.Item3, t.Second));
	}
}
