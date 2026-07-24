namespace MentorLake.Redux.Tests;

public record TestEntity(string Id, string Name) : IKeyed<string>;

[TestClass]
public class DataTableTests
{
	[TestMethod]
	public void UpsertOne_ByKey_AddsEntryAndId()
	{
		var table = new DataTable<string, string>()
			.UpsertOne("a", "alpha");

		Assert.IsTrue(table.ContainsKey("a"));
		Assert.AreEqual("alpha", table.Get("a"));
		CollectionAssert.AreEqual(new[] { "a" }, table.AllIds.ToArray());
	}

	[TestMethod]
	public void UpsertOne_ByKey_UpdatesExistingWithoutDuplicatingId()
	{
		var table = new DataTable<string, string>()
			.UpsertOne("a", "alpha")
			.UpsertOne("a", "ALPHA");

		Assert.AreEqual("ALPHA", table.Get("a"));
		CollectionAssert.AreEqual(new[] { "a" }, table.AllIds.ToArray());
	}

	[TestMethod]
	public void UpsertOne_Keyed_AddsAndUpdates()
	{
		var table = new DataTable<string, TestEntity>()
			.UpsertOne(new TestEntity("1", "one"))
			.UpsertOne(new TestEntity("1", "ONE"))
			.UpsertOne(new TestEntity("2", "two"));

		Assert.AreEqual("ONE", table.Get("1").Name);
		Assert.AreEqual("two", table.Get("2").Name);
		CollectionAssert.AreEqual(new[] { "1", "2" }, table.AllIds.ToArray());
	}

	[TestMethod]
	public void UpsertMany_Keyed()
	{
		var table = new DataTable<string, TestEntity>()
			.UpsertMany(new[]
			{
				new TestEntity("1", "one"),
				new TestEntity("2", "two")
			});

		Assert.AreEqual(2, table.ById.Count);
		Assert.AreEqual("one", table.Get("1").Name);
	}

	[TestMethod]
	public void UpsertMany_KeyValuePairs()
	{
		var table = new DataTable<int, string>()
			.UpsertMany(new[]
			{
				new KeyValuePair<int, string>(1, "a"),
				new KeyValuePair<int, string>(2, "b")
			});

		Assert.AreEqual("a", table.Get(1));
		Assert.AreEqual("b", table.Get(2));
	}

	[TestMethod]
	public void UpsertMany_Tuples()
	{
		var table = new DataTable<int, string>()
			.UpsertMany(new[]
			{
				Tuple.Create(1, "a"),
				Tuple.Create(2, "b")
			});

		Assert.AreEqual("a", table.Get(1));
		Assert.AreEqual("b", table.Get(2));
	}

	[TestMethod]
	public void UpsertMany_ValueTuples()
	{
		var table = new DataTable<int, string>()
			.UpsertMany(new (int, string)[] { (1, "a"), (2, "b") });

		Assert.AreEqual("a", table.Get(1));
		Assert.AreEqual("b", table.Get(2));
	}

	[TestMethod]
	public void Remove_ByKey()
	{
		var table = new DataTable<string, string>()
			.UpsertOne("a", "alpha")
			.UpsertOne("b", "beta")
			.Remove("a");

		Assert.IsFalse(table.ContainsKey("a"));
		Assert.IsTrue(table.ContainsKey("b"));
		CollectionAssert.AreEqual(new[] { "b" }, table.AllIds.ToArray());
	}

	[TestMethod]
	public void Remove_Keyed()
	{
		var entity = new TestEntity("1", "one");
		var table = new DataTable<string, TestEntity>()
			.UpsertOne(entity)
			.Remove(entity);

		Assert.IsFalse(table.ContainsKey("1"));
		Assert.AreEqual(0, table.AllIds.Count);
	}

	[TestMethod]
	public void GetMany_ReturnsValuesInKeyOrder()
	{
		var table = new DataTable<string, string>()
			.UpsertOne("a", "alpha")
			.UpsertOne("b", "beta")
			.UpsertOne("c", "gamma");

		var values = table.GetMany(new[] { "c", "a" }).ToArray();
		CollectionAssert.AreEqual(new[] { "gamma", "alpha" }, values);
	}

	[TestMethod]
	public void Equals_UsesReferenceEquality()
	{
		var table1 = new DataTable<string, string>().UpsertOne("a", "alpha");
		var table2 = new DataTable<string, string>().UpsertOne("a", "alpha");
		var same = table1;

		Assert.IsTrue(table1.Equals(same));
		Assert.IsFalse(table1.Equals(table2));
		Assert.IsFalse(table1.Equals(null));
	}
}
