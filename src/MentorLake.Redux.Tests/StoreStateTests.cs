namespace MentorLake.Redux.Tests;

[TestClass]
public class StoreStateTests
{
	[TestMethod]
	public void UpdateAndGetFeatureState()
	{
		var state = new StoreState()
			.UpdateFeatureState(new PersonState("Ada", "Lovelace"));

		var person = state.GetFeatureState<PersonState>();
		Assert.AreEqual("Ada", person.FirstName);
		Assert.AreEqual("Lovelace", person.LastName);
	}

	[TestMethod]
	[ExpectedException(typeof(KeyNotFoundException))]
	public void GetFeatureState_MissingKey_Throws()
	{
		new StoreState().GetFeatureState<PersonState>();
	}

	[TestMethod]
	public void UpdateFeatureState_IsImmutable()
	{
		var original = new StoreState()
			.UpdateFeatureState(new PersonState("Ada", "Lovelace"));
		var updated = original.UpdateFeatureState(new PersonState("Grace", "Hopper"));

		Assert.AreEqual("Ada", original.GetFeatureState<PersonState>().FirstName);
		Assert.AreEqual("Grace", updated.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public void UpdateFeatureState_OverwritesExistingKey()
	{
		var state = new StoreState()
			.UpdateFeatureState(new PersonState("First", "Last"))
			.UpdateFeatureState(new PersonState("Second", "Last"));

		Assert.AreEqual("Second", state.GetFeatureState<PersonState>().FirstName);
	}

	[TestMethod]
	public void UpdateFeatureState_PreservesOtherFeatures()
	{
		var state = new StoreState()
			.UpdateFeatureState(new PersonState("Ada", "Lovelace"))
			.UpdateFeatureState(new AddressState("02139"));

		Assert.AreEqual("Ada", state.GetFeatureState<PersonState>().FirstName);
		Assert.AreEqual("02139", state.GetFeatureState<AddressState>().ZipCode);
	}

	[TestMethod]
	public void Equals_UsesReferenceEquality()
	{
		var state1 = new StoreState().UpdateFeatureState(new PersonState("A", "B"));
		var state2 = new StoreState().UpdateFeatureState(new PersonState("A", "B"));
		var same = state1;

		Assert.IsTrue(state1.Equals(same));
		Assert.IsFalse(state1.Equals(state2));
		Assert.IsFalse(state1.Equals(null));
	}
}
