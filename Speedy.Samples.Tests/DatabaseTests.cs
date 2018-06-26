#region References

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests
{
	[TestClass]
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public class DatabaseTests
	{
		#region Methods

		[TestMethod]
		public void AddEntity()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
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
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					Food food;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						food = new Food { Name = "Bourbon Reduction", ChildRelationships = new[] { new FoodRelationship { Child = new Food { Name = "Bourbon" }, Quantity = 2 } } };

						database.Food.Add(food);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var actual = EntityFrameworkQueryableExtensions.Include(database.Food, x => x.ChildRelationships)
							.ThenInclude(x => x.Child)
							.First(x => x.Name == food.Name);

						actual = database.Food
							.Include(x => x.ChildRelationships)
							.ThenInclude(x => x.Child)
							.First(x => x.Name == food.Name);

						var children = actual.ChildRelationships.ToList();

						Assert.AreEqual("Bourbon Reduction", actual.Name);
						Assert.AreEqual(1, actual.Id);
						Assert.AreEqual(1, children.Count);
						Assert.AreEqual("Bourbon", children[0].Child.Name);
						Assert.AreEqual(2, children[0].ChildId);
						Assert.AreEqual(2, children[0].Quantity);
						Assert.AreEqual(1, children[0].ParentId);
					}
				});
		}

		[TestMethod]
		public void AddEntityWithCompositeKey()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						var expected = new Person { Name = "Foo", Address = address };
						database.People.Add(expected);
						database.SaveChanges();

						var pet = new Pet { Name = "Max", Owner = expected, Type = new PetType { Id = "Dog", Type = "Boston" } };
						database.Pets.Add(pet);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var owner = database.People.Including(x => x.Owners).First();
						var actual = owner.Owners.First();

						Assert.AreEqual("Max", actual.Name);
						Assert.AreEqual(owner.Id, actual.OwnerId);
					}
				});
		}

		[TestMethod]
		public void AddEntityWithInvalidProperty()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.Addresses.Add(new Address { City = "City", Line2 = "Line2", Postal = "Postal", State = "State" });
						var test = database.Addresses.ToList();
						Assert.AreEqual(0, test.Count);

						// ReSharper disable once AccessToDisposedClosure
						TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
							"Address: The Line1 field is required.",
							"Cannot insert the value NULL into column 'Line1'");
					}
				});
		}

		[TestMethod]
		public void AddEntityWithoutMaintainDatesDatabaseOption()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						database.Options.MaintainDates = false;

						var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
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
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						database.Options.UnmaintainEntities = new[] { typeof(Address) };

						var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
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
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new Person { Name = "John Doe" });
						database.Addresses.Add(address);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreNotEqual(0, database.Addresses.First().Id);
						Assert.AreEqual(1, database.People.Count());
						Assert.AreNotEqual(0, database.People.First().Id);
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreNotEqual(0, database.Addresses.First().People.First().AddressId);
					}
				});
		}

		[TestMethod]
		public void AddNonModifiableEntity()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var expected = new LogEvent { Message = "The new log message that is really important.", Id = "Message" };

						database.LogEvents.Add(expected);
						var actual = database.LogEvents.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.LogEvents.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreNotEqual(default(DateTime), actual.CreatedOn);
						TestHelper.AreEqual(expected, actual, nameof(Address.CreatedOn), nameof(Address.ModifiedOn));
					}
				});
		}

		[TestMethod]
		public void AddSingleEntity()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.Addresses.Add(new Address
						{
							Line1 = "123 Main Street",
							Line2 = string.Empty,
							City = "Easley",
							State = "SC",
							Postal = "29640"
						});

						Assert.AreEqual(0, database.Addresses.Count());

						var count = database.SaveChanges();

						Assert.AreEqual(1, count);
						Assert.AreEqual(1, database.Addresses.Count());
					}
				});
		}

		[TestMethod]
		public void AddSingleEntityMissingRequiredProperty()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						database.Addresses.Add(new Address { Line1 = null, Line2 = string.Empty, City = "Easley", State = "SC", Postal = "29640" });
						TestHelper.ExpectedException<Exception>(() => database.SaveChanges(), "Address: The Line1 field is required.", "Cannot insert the value NULL into column 'Line1'");
					}
				});
		}

		[TestMethod]
		public void AddSingleEntityUsingRelationship()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(address);

						var address2 = new Address { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(address2);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(2, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						address.People.Add(new Person { Name = "John Doe" });
						Assert.AreEqual(1, address.People.Count);

						database.SaveChanges();

						Assert.AreEqual(2, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(1, database.People.Count());
					}
				});
		}

		[TestMethod]
		public void AddTwoEntities()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(1, actual.Id);
						Assert.AreNotEqual(default(DateTime), actual.CreatedOn);
						TestHelper.AreEqual(expected, actual);

						expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(expected);
						database.SaveChanges();

						actual = database.Addresses.OrderBy(x => x.Id).Skip(1).FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(2, actual.Id);
						Assert.AreNotEqual(default(DateTime), actual.CreatedOn);
						TestHelper.AreEqual(expected, actual, nameof(Address.CreatedOn), nameof(Address.ModifiedOn));
					}
				});
		}

		[TestMethod]
		public void DiscardChanges()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.DiscardChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);
					}
				});
		}

		[TestMethod]
		public void EntitiesWithInterfaceShouldStillSerialize()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					var expected = new LogEvent { Message = "This is a test.", Id = "Test" };

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.LogEvents.Add(expected);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.LogEvents.First();
						TestHelper.AreEqual(expected, actual, nameof(Address.CreatedOn), nameof(Address.ModifiedOn));
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
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address1 = new Address { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
						var address2 = new Address { City = "City2", Line1 = "Line2", Line2 = "Line2", Postal = "Postal2", State = "State2" };
						address1.LinkedAddress = address2;
						database.Addresses.Add(address1);
						database.SaveChanges();

						var expected = new[] { address2, address1 };
						var actual = database.Addresses.ToArray();
						TestHelper.AreEqual(expected, actual, nameof(Address.CreatedOn), nameof(Address.ModifiedOn));
					}
				});
		}

		[TestMethod]
		public void QueryUsingInclude()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new Person { Name = "John Doe", Address = address });
					database.Addresses.Add(address);

					var address2 = new Address { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address2);
					address2.People.Add(new Person { Name = "Jane Doe", Address = address2 });

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.People.Count());

					database.SaveChanges();

					Assert.AreEqual(2, database.Addresses.Count());
					Assert.AreEqual(2, database.People.Count());
					Assert.AreEqual(1, database.Addresses.First().People.Count);
				}
			});

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var person = database.People
						.Include(x => x.Address)
						.First(x => x.Name == "John Doe");

					Assert.IsTrue(person.Address != null);
				}
			});
		}

		[TestMethod]
		public void QueryUsingRecursiveRelationshipsUsingExtensionMethods()
		{
			TestHelper.GetDataContextProviders()
				.ForEach(provider =>
				{
					var length = 20;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 0; i < length; i++)
						{
							var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
							address.People.Add(new Person { Name = "John Doe #" + i, Address = address });
							database.Addresses.Add(address);
						}

						database.SaveChanges();
						Assert.AreEqual(length, database.Addresses.Count());
						Assert.AreEqual(length, database.People.Count());
					}

					using (var database = provider.GetDatabase())
					{
						var address = database.Addresses
							.Where(x => x.People.Any())
							.GetRandomItem();

						Assert.IsNotNull(address);
					}
				});
		}

		[TestMethod]
		public void RelationshipCollectionsCountShouldBeCorrect()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						var person = new Person { Name = "John Doe" };
						var group = new Group { Description = "For those who are epic and code.", Name = "Epic Coders" };
						person.Groups.Add(new GroupMember { Role = "Leader", Group = group });
						address.People.Add(person);
						database.Addresses.Add(address);

						Assert.AreEqual(1, address.People.Count);
						Assert.AreEqual(1, person.Groups.Count);
						Assert.AreEqual(1, group.Members.Count);
						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());
						Assert.AreEqual(0, database.GroupMembers.Count());
						Assert.AreEqual(0, database.Groups.Count());

						database.SaveChanges();

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreNotEqual(0, database.Addresses.First().Id);
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(1, database.People.Count());
						Assert.AreNotEqual(0, database.People.First().Id);
						Assert.AreEqual(1, database.People.First().Groups.Count);
						Assert.AreEqual(1, database.GroupMembers.Count());
						Assert.AreNotEqual(0, database.GroupMembers.First().Id);
						Assert.AreEqual(1, database.Groups.Count());
						Assert.AreNotEqual(0, database.Groups.First().Id);
					}
				});
		}

		[TestMethod]
		public void RelationshipCollectionsShouldFilter()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new Person { Name = "John Doe", Address = address });
						database.Addresses.Add(address);

						var address2 = new Address { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(address2);
						address2.People.Add(new Person { Name = "Jane Doe", Address = address2 });

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(2, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(2, database.People.Count());
					}
				});
		}

		[TestMethod]
		public void RemoveEntity()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
						Assert.AreEqual(0, database.Addresses.Count());

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count());

						var address = database.Addresses.First();
						database.Addresses.Remove(address);
						Assert.AreEqual(1, database.Addresses.Count());

						database.SaveChanges();
						Assert.AreEqual(0, database.Addresses.Count());
					}
				});
		}

		[TestMethod]
		public void RemoveEntityById()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
						Assert.AreEqual(0, database.Addresses.Count());

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count());

						var address = database.Addresses.First();
						database.Addresses.Remove(address.Id);
						Assert.AreEqual(1, database.Addresses.Count());

						database.SaveChanges();
						Assert.AreEqual(0, database.Addresses.Count());
					}
				});
		}

		[TestMethod]
		public void RemoveEntityByIdWithDetachedEntity()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					database.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, database.Addresses.Count());

					database.SaveChanges();
					Assert.AreEqual(1, database.Addresses.Count());
				}
			});

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var address = database.Addresses.First();
					database.Addresses.Remove(address.Id);
					Assert.AreEqual(1, database.Addresses.Count());

					database.SaveChanges();
					Assert.AreEqual(0, database.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void RemoveSingleEntityDependantRelationship()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new Person { Name = "John Doe" });
						database.Addresses.Add(address);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(1, database.People.Count());

						database.Addresses.Remove(address);

						// ReSharper disable once AccessToDisposedClosure
						TestHelper.ExpectedException<DbUpdateException>(() => database.SaveChanges(), "The DELETE statement conflicted with the REFERENCE constraint");
					}
				});
		}

		[TestMethod]
		public void RemoveSingleEntityRelationship()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new Person { Name = "John Doe" });
						database.Addresses.Add(address);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(1, database.People.Count());

						database.People.Remove(address.People.First());
						database.SaveChanges();

						// Note: https://github.com/aspnet/EntityFrameworkCore/issues/12114, https://github.com/aspnet/EntityFrameworkCore/issues/8172
						// Addresses -> People is not being updated...

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(0, database.Addresses.First().People.Count);
						Assert.AreEqual(0, database.People.Count());
					}
				});
		}

		[TestMethod]
		public void SubRelationships()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var pepper = new Food { Name = "Pepper" };
						var salt = new Food { Name = "Salt" };

						var steak = new Food
						{
							Name = "Steak",
							ChildRelationships = new[]
							{
								new FoodRelationship { Child = pepper, Quantity = 1 },
								new FoodRelationship { Child = salt, Quantity = 1 }
							}
						};

						database.Food.Add(steak);
						database.SaveChanges();

						var actual = database.Food.First(x => x.Name == "Steak");
						Assert.AreEqual(steak.Id, actual.Id);

						var children = actual.ChildRelationships.ToList();
						Assert.AreEqual(2, children.Count);
						Assert.AreEqual(pepper.Name, children[0].Child.Name);
						Assert.AreEqual(salt.Name, children[1].Child.Name);
					}
				});
		}

		[TestMethod]
		public void UniqueConstraintsForString()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var expected = new Person { Name = "Foo", Address = NewAddress("Bar") };
						database.People.Add(expected);
						database.SaveChanges();

						expected = new Person { Name = "Foo", Address = NewAddress("Bar") };
						database.People.Add(expected);

						// ReSharper disable once AccessToDisposedClosure
						TestHelper.ExpectedException<Exception>(() => database.SaveChanges(), "The duplicate key value is");
					}
				});
		}

		[TestMethod]
		public void UpdateEntityCreatedOnShouldReset()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);
						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						TestHelper.AreEqual(expected, actual, nameof(Address.CreatedOn), nameof(Address.ModifiedOn));

						var originalDate = actual.CreatedOn;
						actual.CreatedOn = DateTime.MaxValue;
						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
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
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					database.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, database.Addresses.Count());
					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreNotEqual(0, database.Addresses.First().Id);

					var address1 = database.Addresses.First();
					address1.Line1 = "Line One";

					var address2 = database.Addresses.First();
					TestHelper.AreEqual(address1, address2);
				}
			});

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var address1 = database.Addresses.First();
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
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					database.Addresses.Add(new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
					Assert.AreEqual(0, database.Addresses.Count());
					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreNotEqual(0, database.Addresses.First().Id);

					var address1 = database.Addresses.First();
					address1.Line1 = "Line One";

					var address2 = database.Addresses.First();
					TestHelper.AreEqual(address1, address2);

					database.SaveChanges();
				}
			});

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var address1 = database.Addresses.First();
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