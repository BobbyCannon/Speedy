#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class DatabaseTests
	{
		#region Methods

		[TestMethod]
		public void AddEntity()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

					context.Addresses.Add(expected);
					var actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);

					context.SaveChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(0, actual.Id);
					Assert.AreEqual(1, actual.Id);
					Assert.AreNotEqual(default(DateTime), actual.CreatedOn);
					TestHelper.AreEqual(expected, actual);
				}
			});
		}

		[TestMethod]
		public void AddEntityViaSubRelationships()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				Food food;

				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					food = new Food
					{
						Name = "Bourbon Reduction",
						Children = new[]
						{
							new FoodRelationship
							{
								Child = new Food
								{
									Name = "Bourbon"
								},
								Quantity = 2
							}
						}
					};

					database.Foods.Add(food);
					database.SaveChanges();
				}

				using (var database = provider.GetDatabase())
				{
					var actual = database.Foods.Include(x => x.Children).First(x => x.Name == food.Name);
					var children = actual.Children.ToList();

					Assert.AreEqual("Bourbon Reduction", actual.Name);
					Assert.AreEqual(2, actual.Id);
					Assert.AreEqual(1, children.Count);
					Assert.AreEqual("Bourbon", children[0].Child.Name);
					Assert.AreEqual(1, children[0].ChildId);
					Assert.AreEqual(2, children[0].Quantity);
				}
			});
		}

		[TestMethod]
		public void AddEntityWithInvalidProperty()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line2 = "Line2", Postal = "Postal", State = "State" });
					var test = context.Addresses.ToList();
					Assert.AreEqual(0, test.Count);

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => context.SaveChanges(),
						"Address: The Line1 field is required.",
						"Cannot insert the value NULL into column 'Line1'");
				}
			});
		}

		[TestMethod]
		public void AddEntityWithoutMaintainDatesDatabaseOption()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);
					context.Options.MaintainDates = false;

					var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

					context.Addresses.Add(expected);
					var actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);

					context.SaveChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(0, actual.Id);
					Assert.AreEqual(default(DateTime), actual.CreatedOn);
					TestHelper.AreEqual(expected, actual);
				}
			});
		}

		[TestMethod]
		public void AddEntityWithUnmaintainEntityDatabaseOption()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);
					context.Options.UnmaintainEntities = new[] { typeof(Address) };

					var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

					context.Addresses.Add(expected);
					var actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);

					context.SaveChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(0, actual.Id);
					Assert.AreEqual(default(DateTime), actual.CreatedOn);
					Assert.AreEqual(default(DateTime), actual.ModifiedOn);
					TestHelper.AreEqual(expected, actual);
				}
			});
		}

		[TestMethod]
		public void AddMultipleEntitiesUsingRelationship()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new Person { Name = "John Doe" });
					context.Addresses.Add(address);

					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreNotEqual(0, context.Addresses.First().Id);
					Assert.AreEqual(1, context.People.Count());
					Assert.AreNotEqual(0, context.People.First().Id);
					Assert.AreEqual(1, context.Addresses.First().People.Count);
					Assert.AreNotEqual(0, context.Addresses.First().People.First().AddressId);
				}
			});
		}

		[TestMethod]
		public void AddNonModifiableEntity()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var expected = new LogEvent { Message = "The new log message that is really important." };

					context.LogEvents.Add(expected);
					var actual = context.LogEvents.FirstOrDefault();
					Assert.IsNull(actual);

					context.SaveChanges();

					actual = context.LogEvents.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(0, actual.Id);
					Assert.AreNotEqual(default(DateTime), actual.CreatedOn);
					TestHelper.AreEqual(expected, actual);
				}
			});
		}

		[TestMethod]
		public void AddSingleEntity()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address
					{
						Line1 = "123 Main Street",
						Line2 = string.Empty,
						City = "Easley",
						State = "SC",
						Postal = "29640"
					});

					Assert.AreEqual(0, context.Addresses.Count());

					var count = context.SaveChanges();

					Assert.AreEqual(1, count);
					Assert.AreEqual(1, context.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void AddSingleEntityMissingRequiredProperty()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address
					{
						Line1 = null,
						Line2 = string.Empty,
						City = "Easley",
						State = "SC",
						Postal = "29640"
					});

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => context.SaveChanges(),
						"Address: The Line1 field is required.",
						"Cannot insert the value NULL into column 'Line1'");
				}
			});
		}

		[TestMethod]
		public void AddSingleEntityUsingRelationship()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					context.Addresses.Add(address);

					var address2 = new Address { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					context.Addresses.Add(address2);

					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					context.SaveChanges();

					Assert.AreEqual(2, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					address.People.Add(new Person { Name = "John Doe" });
					Assert.AreEqual(1, address.People.Count);

					context.SaveChanges();

					Assert.AreEqual(2, context.Addresses.Count());
					Assert.AreEqual(1, context.Addresses.First().People.Count);
					Assert.AreEqual(1, context.People.Count());
				}
			});
		}

		[TestMethod]
		public void DiscardChanges()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

					context.Addresses.Add(expected);
					var actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);

					context.DiscardChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);

					context.SaveChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);
				}
			});
		}

		[TestMethod]
		public void EntitiesWithInterfaceShouldStillSerialize()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				var expected = new LogEvent { Message = "This is a test." };

				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.LogEvents.Add(expected);
					context.SaveChanges();
				}

				using (var context = provider.GetDatabase())
				{
					var actual = context.LogEvents.First();
					TestHelper.AreEqual(expected, actual);
				}
			});
		}

		[TestInitialize]
		public void Initialize()
		{
			TestHelper.Initialize();
		}

		[TestMethod]
		public void OptionalDirectRelationship()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address1 = new Address { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
					var address2 = new Address { City = "City2", Line1 = "Line2", Line2 = "Line2", Postal = "Postal2", State = "State2" };
					address1.LinkedAddress = address2;
					context.Addresses.Add(address1);
					context.SaveChanges();

					var expected = new[] { address2, address1 };
					var actual = context.Addresses.ToArray();
					TestHelper.AreEqual(expected, actual);
				}
			});
		}

		[TestMethod]
		public void QueryUsingInclude()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new Person { Name = "John Doe", Address = address });
					context.Addresses.Add(address);

					var address2 = new Address { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					context.Addresses.Add(address2);
					address2.People.Add(new Person { Name = "Jane Doe", Address = address2 });

					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					context.SaveChanges();

					Assert.AreEqual(2, context.Addresses.Count());
					Assert.AreEqual(2, context.People.Count());
					Assert.AreEqual(1, context.Addresses.First().People.Count);
				}
			});

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var person = context.People
						.Include(x => x.Address)
						.First(x => x.Name == "John Doe");

					Assert.IsTrue(person.Address != null);
				}
			});
		}

		[TestMethod]
		public void QueryUsingRecursiveRelationshipsUsingExtensionMethods()
		{
			TestHelper.GetDataContextProviders().ForEach(provider =>
			{
				var length = 20;

				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					for (var i = 0; i < length; i++)
					{
						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new Person { Name = "John Doe #" + i, Address = address });
						context.Addresses.Add(address);
					}

					context.SaveChanges();
					Assert.AreEqual(length, context.Addresses.Count());
					Assert.AreEqual(length, context.People.Count());
				}

				using (var context = provider.GetDatabase())
				{
					var address = context.Addresses
						.Where(x => x.People.Any())
						.GetRandomItem();

					Assert.IsNotNull(address);
				}
			});
		}

		[TestMethod]
		public void RelationshipCollectionsCountShouldBeCorrect()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					var person = new Person { Name = "John Doe" };
					var group = new Group { Description = "For those who are epic and code.", Name = "Epic Coders" };
					person.Groups.Add(new GroupMember { Role = "Leader", Group = group });
					address.People.Add(person);
					context.Addresses.Add(address);

					Assert.AreEqual(1, address.People.Count);
					Assert.AreEqual(1, person.Groups.Count);
					Assert.AreEqual(1, group.Members.Count);
					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());
					Assert.AreEqual(0, context.GroupMembers.Count());
					Assert.AreEqual(0, context.Groups.Count());

					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreNotEqual(0, context.Addresses.First().Id);
					Assert.AreEqual(1, context.Addresses.First().People.Count);
					Assert.AreEqual(1, context.People.Count());
					Assert.AreNotEqual(0, context.People.First().Id);
					Assert.AreEqual(1, context.People.First().Groups.Count);
					Assert.AreEqual(1, context.GroupMembers.Count());
					Assert.AreNotEqual(0, context.GroupMembers.First().Id);
					Assert.AreEqual(1, context.Groups.Count());
					Assert.AreNotEqual(0, context.Groups.First().Id);
				}
			});
		}

		[TestMethod]
		public void RelationshipCollectionsShouldFilter()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new Person { Name = "John Doe", Address = address });
					context.Addresses.Add(address);

					var address2 = new Address { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					context.Addresses.Add(address2);
					address2.People.Add(new Person { Name = "Jane Doe", Address = address2 });

					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					context.SaveChanges();

					Assert.AreEqual(2, context.Addresses.Count());
					Assert.AreEqual(1, context.Addresses.First().People.Count);
					Assert.AreEqual(2, context.People.Count());
				}
			});
		}

		[TestMethod]
		public void RemoveEntity()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, context.Addresses.Count());

					context.SaveChanges();
					Assert.AreEqual(1, context.Addresses.Count());

					var address = context.Addresses.First();
					context.Addresses.Remove(address);
					Assert.AreEqual(1, context.Addresses.Count());

					context.SaveChanges();
					Assert.AreEqual(0, context.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void RemoveEntityById()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, context.Addresses.Count());

					context.SaveChanges();
					Assert.AreEqual(1, context.Addresses.Count());

					var address = context.Addresses.First();
					context.Addresses.Remove(address.Id);
					Assert.AreEqual(1, context.Addresses.Count());

					context.SaveChanges();
					Assert.AreEqual(0, context.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void RemoveEntityByIdWithDetachedEntity()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, context.Addresses.Count());

					context.SaveChanges();
					Assert.AreEqual(1, context.Addresses.Count());
				}
			});

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = context.Addresses.First();
					context.Addresses.Remove(address.Id);
					Assert.AreEqual(1, context.Addresses.Count());

					context.SaveChanges();
					Assert.AreEqual(0, context.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void RemoveSingleEntityDependantRelationship()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new Person { Name = "John Doe" });
					context.Addresses.Add(address);

					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreEqual(1, context.Addresses.First().People.Count);
					Assert.AreEqual(1, context.People.Count());

					context.Addresses.Remove(address);

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<InvalidOperationException>(() => context.SaveChanges(),
						"The operation failed: The relationship could not be changed because one or more of the foreign-key properties is non-nullable. When a change is made to a relationship, the related foreign-key property is set to a null value. If the foreign-key does not support null values, a new relationship must be defined, the foreign-key property must be assigned another non-null value, or the unrelated object must be deleted.",
						"has been severed but the foreign key for this relationship cannot be set to null");
				}
			});
		}

		[TestMethod]
		public void RemoveSingleEntityRelationship()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new Person { Name = "John Doe" });
					context.Addresses.Add(address);

					Assert.AreEqual(0, context.Addresses.Count());
					Assert.AreEqual(0, context.People.Count());

					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreEqual(1, context.Addresses.First().People.Count);
					Assert.AreEqual(1, context.People.Count());

					context.People.Remove(address.People.First());
					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreEqual(0, context.Addresses.First().People.Count);
					Assert.AreEqual(0, context.People.Count());
				}
			});
		}

		[TestMethod]
		public void SubRelationships()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var pepper = new Food { Name = "Pepper" };
					var salt = new Food { Name = "Salt" };

					var steak = new Food
					{
						Name = "Steak",
						Children = new[]
						{
							new FoodRelationship { Child = pepper, Quantity = 1 },
							new FoodRelationship { Child = salt, Quantity = 1 }
						}
					};

					context.Foods.Add(steak);
					context.SaveChanges();

					var actual = context.Foods.First(x => x.Name == "Steak");
					Assert.AreEqual(steak.Id, actual.Id);

					var children = actual.Children.ToList();
					Assert.AreEqual(2, children.Count);
					Assert.AreEqual(pepper.Name, children[0].Child.Name);
					Assert.AreEqual(salt.Name, children[1].Child.Name);
				}
			});
		}

		[TestMethod]
		public void UniqueConstraintsForString()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var expected = new Person { Name = "Foo", Address = NewAddress("Bar") };
					context.People.Add(expected);
					context.SaveChanges();

					expected = new Person { Name = "Foo", Address = NewAddress("Bar") };
					context.People.Add(expected);

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => context.SaveChanges(), "The duplicate key value is");
				}
			});
		}

		[TestMethod]
		public void UpdateEntityCreatedOnShouldReset()
		{
			TestHelper.GetDataContexts().ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

					context.Addresses.Add(expected);
					var actual = context.Addresses.FirstOrDefault();
					Assert.IsNull(actual);
					context.SaveChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(0, actual.Id);
					TestHelper.AreEqual(expected, actual);

					var originalDate = actual.CreatedOn;
					actual.CreatedOn = DateTime.MaxValue;
					context.SaveChanges();

					actual = context.Addresses.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreEqual(originalDate, actual.CreatedOn);
				}
			});
		}

		[TestMethod]
		public void UpdateEntityWithoutSave()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, context.Addresses.Count());
					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreNotEqual(0, context.Addresses.First().Id);

					var address1 = context.Addresses.First();
					address1.Line1 = "Line One";

					var address2 = context.Addresses.First();
					TestHelper.AreEqual(address1, address2);
				}
			});

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address1 = context.Addresses.First();
					Assert.AreEqual("Line1", address1.Line1);
				}
			});
		}

		[TestMethod]
		public void UpdateEntityWithSave()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, context.Addresses.Count());
					context.SaveChanges();

					Assert.AreEqual(1, context.Addresses.Count());
					Assert.AreNotEqual(0, context.Addresses.First().Id);

					var address1 = context.Addresses.First();
					address1.Line1 = "Line One";

					var address2 = context.Addresses.First();
					TestHelper.AreEqual(address1, address2);

					context.SaveChanges();
				}
			});

			providers.ForEach(provider =>
			{
				using (var context = provider.GetDatabase())
				{
					Console.WriteLine(context.GetType().Name);

					var address1 = context.Addresses.First();
					Assert.AreEqual("Line One", address1.Line1);
				}
			});
		}

		private static Address NewAddress(string line1, string line2 = "")
		{
			return new Address { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		#endregion
	}
}