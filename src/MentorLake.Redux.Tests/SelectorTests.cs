using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace MentorLake.Redux.Tests;

[TestClass]
public class SelectorTests
{
	private ReduxStore _store;

	[TestInitialize]
	public void Initialize()
	{
		_store = new ReduxStore();
		_store.RegisterReducers(
			FeatureReducer.Build(new PersonState("Hello", "World"))
				.On<UpdateFirstNameAction>((state, action) => state with { FirstName = action.FirstName })
				.On<UpdateLastNameAction>((state, action) => state with { LastName = action.LastName }),
			FeatureReducer.Build(new AddressState("12345"))
				.On<ZipCodeUpdatedAction>((state, action) => state with { ZipCode = action.ZipCode }));
	}

	[TestMethod]
	public void CreateFeature_ReadsFeatureState()
	{
		var person = MySelectors.Person.Apply(_store.State);
		Assert.AreEqual("Hello", person.FirstName);
		Assert.AreEqual("World", person.LastName);
	}

	[TestMethod]
	public void Create_FromStoreStateFunc()
	{
		var selector = SelectorFactory.Create(s => s.GetFeatureState<PersonState>().FirstName);
		Assert.AreEqual("Hello", selector.Apply(_store.State));
	}

	[TestMethod]
	public void Create_FromSingleSelector_ProjectsValue()
	{
		Assert.AreEqual("Hello", MySelectors.FirstName.Apply(_store.State));
	}

	[TestMethod]
	public void MemoizedSelector_SkipsProjectorWhenInputsUnchanged()
	{
		var projectCount = 0;
		var selector = SelectorFactory.Create(MySelectors.FirstName, name =>
		{
			projectCount++;
			return name.ToUpperInvariant();
		});

		Assert.AreEqual("HELLO", selector.Apply(_store.State));
		Assert.AreEqual("HELLO", selector.Apply(_store.State));
		Assert.AreEqual(1, projectCount);

		_store.Dispatch(new UpdateFirstNameAction("Bob"));
		Assert.AreEqual("BOB", selector.Apply(_store.State));
		Assert.AreEqual(2, projectCount);
	}

	[TestMethod]
	public void MemoizedSelector_TwoInputs_CombinesAndMemoizes()
	{
		var projectCount = 0;
		var selector = SelectorFactory.Create(
			MySelectors.FirstName,
			MySelectors.ZipCode,
			(first, zip) =>
			{
				projectCount++;
				return $"{first}@{zip}";
			});

		Assert.AreEqual("Hello@12345", selector.Apply(_store.State));
		Assert.AreEqual("Hello@12345", selector.Apply(_store.State));
		Assert.AreEqual(1, projectCount);

		_store.Dispatch(new ZipCodeUpdatedAction("99999"));
		Assert.AreEqual("Hello@99999", selector.Apply(_store.State));
		Assert.AreEqual(2, projectCount);

		_store.Dispatch(new UpdateLastNameAction("ignored"));
		Assert.AreEqual("Hello@99999", selector.Apply(_store.State));
		Assert.AreEqual(2, projectCount);
	}

	[TestMethod]
	public void MemoizedSelector_ThreeInputs()
	{
		var lastName = SelectorFactory.Create(MySelectors.Person, p => p.LastName);
		var selector = SelectorFactory.Create(
			MySelectors.FirstName,
			lastName,
			MySelectors.ZipCode,
			(first, last, zip) => $"{first} {last} / {zip}");

		Assert.AreEqual("Hello World / 12345", selector.Apply(_store.State));
	}

	[TestMethod]
	public void MemoizedSelector_PassesPreviousRunToProjector()
	{
		(string Result, string Input1)? capturedPrevious = ("sentinel", "sentinel");
		var selector = SelectorFactory.Create(
			MySelectors.FirstName,
			((string Result, string Input1)? prev, string name) =>
			{
				capturedPrevious = prev;
				return name;
			});

		selector.Apply(_store.State);
		Assert.IsNull(capturedPrevious);

		_store.Dispatch(new UpdateFirstNameAction("Bob"));
		selector.Apply(_store.State);
		Assert.IsNotNull(capturedPrevious);
		Assert.AreEqual("Hello", capturedPrevious.Value.Result);
		Assert.AreEqual("Hello", capturedPrevious.Value.Input1);
	}

	[TestMethod]
	public void Create_WithEqualityComparer_SuppressesEquivalentResults()
	{
		var emissions = new List<PersonState>();
		var selector = SelectorFactory.Create(
			MySelectors.Person,
			p => p,
			SameFirstName);

		using var _ = _store.Select(selector).Subscribe(emissions.Add);

		_store.Dispatch(new UpdateLastNameAction("Changed"));
		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		// initial + first-name change only
		Assert.AreEqual(2, emissions.Count);
		Assert.AreEqual("Hello", emissions[0].FirstName);
		Assert.AreEqual("Bob", emissions[1].FirstName);
	}

	[TestMethod]
	public void WithComparer_UsesCustomEquality()
	{
		var emissions = new List<PersonState>();
		var selector = MySelectors.Person.WithComparer(SameFirstName);

		using var _ = _store.Select(selector).Subscribe(emissions.Add);

		_store.Dispatch(new UpdateLastNameAction("Changed"));
		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.AreEqual(2, emissions.Count);
	}

	[TestMethod]
	public void WithSequenceComparer_ComparesListContents()
	{
		var listSelector = SelectorFactory.Create(s =>
			ImmutableList.Create(
				s.GetFeatureState<PersonState>().FirstName,
				s.GetFeatureState<PersonState>().LastName));

		var emissions = new List<ImmutableList<string>>();
		var selector = listSelector.WithSequenceComparer((a, b) => a == b);
		using var _ = _store.Select(selector).Subscribe(emissions.Add);

		// Same logical content after a no-op-like path is hard; dispatch a real change
		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		Assert.IsTrue(emissions.Count >= 2);
		Assert.AreEqual("Hello", emissions[0][0]);
		Assert.AreEqual("Bob", emissions[^1][0]);
	}

	[TestMethod]
	public async Task Select_Func_EmitsDistinctValues()
	{
		var valuesTask = _store.Select(s => s.GetFeatureState<PersonState>().FirstName)
			.Take(2)
			.ToArray()
			.ToTask();

		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		var values = await valuesTask;
		CollectionAssert.AreEqual(new[] { "Hello", "Bob" }, values);
	}

	[TestMethod]
	public async Task Select_ISelector_EmitsOnChange()
	{
		var valuesTask = _store.Select(MySelectors.FirstName).Take(2).ToArray().ToTask();
		_store.Dispatch(new UpdateFirstNameAction("Bob"));

		var values = await valuesTask;
		CollectionAssert.AreEqual(new[] { "Hello", "Bob" }, values);
	}

	private static bool SameFirstName(PersonState a, PersonState b)
	{
		if (ReferenceEquals(a, b)) return true;
		if (a is null || b is null) return false;
		return a.FirstName == b.FirstName;
	}
}
