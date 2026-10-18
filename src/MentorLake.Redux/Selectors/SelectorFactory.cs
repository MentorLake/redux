
namespace MentorLake.Redux.Selectors;

/// <summary>
/// Factory for selectors. Use <see cref="CreateFeature{TFinalResult}"/> for feature roots,
/// <see cref="Create{TFinalResult}(Func{StoreState, TFinalResult}, IEqualityComparer{TFinalResult})"/>
/// for ad-hoc projections, and the <c>Create(s1, ..., proj)</c> overloads (1–7 inputs) to
/// compose memoized selectors from other selectors.
/// </summary>
/// <remarks>
/// The <c>Create(s1, ..., proj)</c> overloads are generated from SelectorFactory.tt —
/// edit the template, not the generated file. Each overload comes in four shapes: with and
/// without previous-result access in the projector, and with an
/// <see cref="IEqualityComparer{T}"/> or a comparison delegate.
/// </remarks>
public static class SelectorFactory
{
	/// <summary>
	/// Root selector for a feature slice — the building block all derived selectors start from.
	/// </summary>
	/// <typeparam name="TFinalResult">The feature state type.</typeparam>
	public static ISelector<TFinalResult> CreateFeature<TFinalResult>() where TFinalResult : class =>
		new SimpleSelector<TFinalResult>(s => s.GetFeatureState<TFinalResult>());

	/// <summary>
	/// Ad-hoc projection straight from <see cref="StoreState"/>. Not memoized across
	/// subscribers — prefer composing from <see cref="CreateFeature{TFinalResult}"/> roots
	/// for shared evaluation.
	/// </summary>
	/// <typeparam name="TFinalResult">The projected value.</typeparam>
	/// <param name="projectorFunction">Pure projection from whole state.</param>
	/// <param name="equalityComparer">Change test for stream emission; defaults to the type's equality.</param>
	public static ISelector<TFinalResult> Create<TFinalResult>(
		Func<StoreState, TFinalResult> projectorFunction,
		IEqualityComparer<TFinalResult> equalityComparer = null) =>
			new SimpleSelector<TFinalResult>(projectorFunction, equalityComparer);

	public static ISelector<TResult> Create<S1,TResult>(
		ISelector<S1> s1,
		Func<(TResult Result, S1 Input1)?,S1,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,TResult>(s1,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,TResult>(
		ISelector<S1> s1,
		Func<(TResult Result, S1 Input1)?,S1,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,TResult>(s1,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,TResult>(
		ISelector<S1> s1,
		Func<S1,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,TResult>(s1,(_,x1) => proj(x1),equalityComparer);

	public static ISelector<TResult> Create<S1,TResult>(
		ISelector<S1> s1,
		Func<S1,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,TResult>(s1,(_,x1) => proj(x1),EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,
		Func<(TResult Result, S1 Input1,S2 Input2)?,S1,S2,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,TResult>(s1,s2,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,S2,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,
		Func<(TResult Result, S1 Input1,S2 Input2)?,S1,S2,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,TResult>(s1,s2,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,
		Func<S1,S2,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,TResult>(s1,s2,(_,x1,x2) => proj(x1,x2),equalityComparer);

	public static ISelector<TResult> Create<S1,S2,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,
		Func<S1,S2,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,TResult>(s1,s2,(_,x1,x2) => proj(x1,x2),EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3)?,S1,S2,S3,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,TResult>(s1,s2,s3,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3)?,S1,S2,S3,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,TResult>(s1,s2,s3,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,
		Func<S1,S2,S3,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,TResult>(s1,s2,s3,(_,x1,x2,x3) => proj(x1,x2,x3),equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,
		Func<S1,S2,S3,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,TResult>(s1,s2,s3,(_,x1,x2,x3) => proj(x1,x2,x3),EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4)?,S1,S2,S3,S4,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,TResult>(s1,s2,s3,s4,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4)?,S1,S2,S3,S4,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,TResult>(s1,s2,s3,s4,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,
		Func<S1,S2,S3,S4,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,TResult>(s1,s2,s3,s4,(_,x1,x2,x3,x4) => proj(x1,x2,x3,x4),equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,
		Func<S1,S2,S3,S4,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,TResult>(s1,s2,s3,s4,(_,x1,x2,x3,x4) => proj(x1,x2,x3,x4),EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4,S5 Input5)?,S1,S2,S3,S4,S5,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,TResult>(s1,s2,s3,s4,s5,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4,S5 Input5)?,S1,S2,S3,S4,S5,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,TResult>(s1,s2,s3,s4,s5,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,
		Func<S1,S2,S3,S4,S5,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,TResult>(s1,s2,s3,s4,s5,(_,x1,x2,x3,x4,x5) => proj(x1,x2,x3,x4,x5),equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,
		Func<S1,S2,S3,S4,S5,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,TResult>(s1,s2,s3,s4,s5,(_,x1,x2,x3,x4,x5) => proj(x1,x2,x3,x4,x5),EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4,S5 Input5,S6 Input6)?,S1,S2,S3,S4,S5,S6,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,TResult>(s1,s2,s3,s4,s5,s6,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4,S5 Input5,S6 Input6)?,S1,S2,S3,S4,S5,S6,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,TResult>(s1,s2,s3,s4,s5,s6,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,
		Func<S1,S2,S3,S4,S5,S6,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,TResult>(s1,s2,s3,s4,s5,s6,(_,x1,x2,x3,x4,x5,x6) => proj(x1,x2,x3,x4,x5,x6),equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,
		Func<S1,S2,S3,S4,S5,S6,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,TResult>(s1,s2,s3,s4,s5,s6,(_,x1,x2,x3,x4,x5,x6) => proj(x1,x2,x3,x4,x5,x6),EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,S7,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,ISelector<S7> s7,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4,S5 Input5,S6 Input6,S7 Input7)?,S1,S2,S3,S4,S5,S6,S7,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,S7,TResult>(s1,s2,s3,s4,s5,s6,s7,proj,equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,S7,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,ISelector<S7> s7,
		Func<(TResult Result, S1 Input1,S2 Input2,S3 Input3,S4 Input4,S5 Input5,S6 Input6,S7 Input7)?,S1,S2,S3,S4,S5,S6,S7,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,S7,TResult>(s1,s2,s3,s4,s5,s6,s7,proj,EqualityComparer<TResult>.Create(equalityComparer));

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,S7,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,ISelector<S7> s7,
		Func<S1,S2,S3,S4,S5,S6,S7,TResult> proj,
		IEqualityComparer<TResult> equalityComparer = null) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,S7,TResult>(s1,s2,s3,s4,s5,s6,s7,(_,x1,x2,x3,x4,x5,x6,x7) => proj(x1,x2,x3,x4,x5,x6,x7),equalityComparer);

	public static ISelector<TResult> Create<S1,S2,S3,S4,S5,S6,S7,TResult>(
		ISelector<S1> s1,ISelector<S2> s2,ISelector<S3> s3,ISelector<S4> s4,ISelector<S5> s5,ISelector<S6> s6,ISelector<S7> s7,
		Func<S1,S2,S3,S4,S5,S6,S7,TResult> proj,
		Func<TResult,TResult,bool> equalityComparer) =>
			 new MemoizedSelector<S1,S2,S3,S4,S5,S6,S7,TResult>(s1,s2,s3,s4,s5,s6,s7,(_,x1,x2,x3,x4,x5,x6,x7) => proj(x1,x2,x3,x4,x5,x6,x7),EqualityComparer<TResult>.Create(equalityComparer));
}

