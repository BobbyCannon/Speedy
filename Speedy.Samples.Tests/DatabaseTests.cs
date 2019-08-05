#region References

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Exceptions;
using Speedy.Samples.Entities;
using Speedy.Storage;
using Speedy.Tests.EntityFactories;

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

						var tracker = new CollectionChangeTracker();
						var expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.CollectionChanged += (sender, args) => tracker.Update(args);
						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						database.SaveChanges();
						Assert.AreEqual(1, tracker.Added.Count);
						Assert.AreEqual(expected, tracker.Added[0]);
						Assert.AreEqual(0, tracker.Removed.Count);

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(1, actual.Id);
						Assert.AreNotEqual(default, actual.CreatedOn);
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
					FoodEntity food;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						food = new FoodEntity { Name = "Bourbon Reduction", ChildRelationships = new[] { new FoodRelationshipEntity { Child = new FoodEntity { Name = "Bourbon" }, Quantity = 2 } } };

						database.Food.Add(food);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.Food
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

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						var expected = new PersonEntity { Name = "Foo", Address = address };
						database.People.Add(expected);
						database.SaveChanges();

						var pet = new PetEntity { Name = "Max", Owner = expected, Type = new PetTypeEntity { Id = "Dog", Type = "Boston" } };
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

						database.Addresses.Add(new AddressEntity { City = "City", Line2 = "Line2", Postal = "Postal", State = "State" });
						var test = database.Addresses.ToList();
						Assert.AreEqual(0, test.Count);

						// ReSharper disable once AccessToDisposedClosure
						TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
							"SQLite Error 19: 'NOT NULL constraint failed: Addresses.Line1'.",
							"Cannot insert the value NULL into column 'Line1', table 'Speedy.dbo.Addresses'; column does not allow nulls. INSERT fails.",
							"AddressEntity: The Line1 field is required.");
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
						database.Options.MaintainCreatedOn = false;
						database.Options.MaintainModifiedOn = false;

						var expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(default, actual.CreatedOn);
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
						database.Options.UnmaintainEntities = new[] { typeof(AddressEntity) };

						var expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(default, actual.CreatedOn);
						Assert.AreEqual(default, actual.ModifiedOn);
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

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new PersonEntity { Name = "John Doe" });
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

						var expected = new LogEventEntity { Message = "The new log message that is really important.", Id = "Message" };

						database.LogEvents.Add(expected);
						var actual = database.LogEvents.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.LogEvents.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreNotEqual(default, actual.CreatedOn);
						TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));
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

						database.Addresses.Add(new AddressEntity
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
						database.Addresses.Add(new AddressEntity { Line1 = null, Line2 = string.Empty, City = "Easley", State = "SC", Postal = "29640" });
						TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
							"SQLite Error 19: 'NOT NULL constraint failed: Addresses.Line1'.",
							"Cannot insert the value NULL into column 'Line1', table 'Speedy.dbo.Addresses'; column does not allow nulls. INSERT fails.",
							"AddressEntity: The Line1 field is required.");
					}
				});
		}

		[TestMethod]
		public void AddSingleEntityMissingRequiredRelationship()
		{
			// todo: finish this test
			//TestHelper.GetDataContexts()
			//	.ForEach(provider =>
			//	{
			//		using (var database = provider.GetDatabase())
			//		{
			//			Console.WriteLine(database.GetType().Name);
			//			database.People.Add(new PersonEntity { Name = "John" });
			//			TestHelper.ExpectedException<Exception>(() => database.SaveChanges(), "The INSERT statement conflicted with the FOREIGN KEY constraint");
			//		}
			//	});
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

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(address);

						var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(address2);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(2, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						address.People.Add(new PersonEntity { Name = "John Doe" });
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

						var expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(1, actual.Id);
						Assert.AreNotEqual(default, actual.CreatedOn);
						TestHelper.AreEqual(expected, actual);

						expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(expected);
						database.SaveChanges();

						actual = database.Addresses.OrderBy(x => x.Id).Skip(1).FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						Assert.AreEqual(2, actual.Id);
						Assert.AreNotEqual(default, actual.CreatedOn);
						TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));
					}
				});
		}

		[TestMethod]
		public void BulkDeleteShouldDelete()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 0; i < 10; i++)
						{
							var address = new AddressEntity { City = $"City{i}", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
							database.Addresses.Add(address);
						}

						database.SaveChanges();

						Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(10, database.Addresses.Count(x => !x.IsDeleted));

						var count = database.Addresses.BulkRemove(x => !x.IsDeleted);

						Assert.AreEqual(10, count);
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
					}
				}
			);
		}
		
		[TestMethod]
		public void BulkDeleteShouldDeletePartial()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 0; i < 10; i++)
						{
							var address = new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
							database.Addresses.Add(address);
						}

						database.SaveChanges();

						Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(10, database.Addresses.Count(x => !x.IsDeleted));

						var count = database.Addresses.BulkRemove(x => x.Id >= 5);

						Assert.AreEqual(6, count);
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(4, database.Addresses.Count(x => !x.IsDeleted));
					}
				}
			);
		}
		
		[TestMethod]
		public void BulkUpdateShouldUpdate()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 0; i < 10; i++)
						{
							var address = new AddressEntity { City = $"City{i}", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
							database.Addresses.Add(address);
						}

						database.SaveChanges();

						Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));

						var count = database.Addresses.BulkUpdate(x => !x.IsDeleted, x => new AddressEntity { IsDeleted = true });
						Assert.AreEqual(10, count);
						Assert.AreEqual(10, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
						
						count = database.Addresses.BulkUpdate(x => x.Id < 5 && x.Id >= 2, x => new AddressEntity { City = "city" });
						Assert.AreEqual(3, count);
						Assert.AreEqual(3, database.Addresses.Count(x => x.City == "city"));
						Assert.AreEqual(7, database.Addresses.Count(x => x.City != "city"));
					}
				}
			);
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

						var expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

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
					var expected = new LogEventEntity { Message = "This is a test.", Id = "Test" };

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.LogEvents.Add(expected);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.LogEvents.ToList().First().Unwrap<LogEventEntity>();
						TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));
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

						var address1 = new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
						var address2 = new AddressEntity { City = "City2", Line1 = "Line2", Line2 = "Line2", Postal = "Postal2", State = "State2" };
						address1.LinkedAddress = address2;
						database.Addresses.Add(address1);
						database.SaveChanges();

						var expected = new[] { address2, address1 };
						var actual = database.Addresses.ToArray();
						TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));
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

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new PersonEntity { Name = "John Doe", Address = address });
					database.Addresses.Add(address);

					var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address2);
					address2.People.Add(new PersonEntity { Name = "Jane Doe", Address = address2 });

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
		public void QueryUsingIncluding()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.People.Add(new PersonEntity { Name = "John Doe", Address = address });
					database.Addresses.Add(address);

					var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address2);
					address2.People.Add(new PersonEntity { Name = "Jane Doe", Address = address2 });

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
						.Including(x => x.Address, x => x.Groups)
						.First(x => x.Name == "John Doe");

					Assert.IsTrue(person.Address != null);
					Assert.IsTrue(person.Groups != null);
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
							var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
							address.People.Add(new PersonEntity { Name = "John Doe #" + i, Address = address });
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
		public void QueryUsingThenInclude()
		{
			var providers = TestHelper.GetDataContextProviders().ToList();

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					var person = new PersonEntity { Name = "John Doe", Address = address };
					person.Groups.Add(new GroupMemberEntity { Group = new GroupEntity { Name = "Group", Description = "Description" }, Role = "Role" });
					address.People.Add(person);
					database.Addresses.Add(address);

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.People.Count());
					Assert.AreEqual(0, database.People.Count());

					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreEqual(1, database.People.Count());
					Assert.AreEqual(1, database.Groups.Count());
					Assert.AreEqual(1, database.GroupMembers.Count());
				}
			});

			providers.ForEach(provider =>
			{
				using (var database = provider.GetDatabase())
				{
					Console.WriteLine(database.GetType().Name);

					var person = database.People
						.Include(x => x.Owners)
						.Include(x => x.Groups)
						.ThenInclude(x => x.Group)
						.First(x => x.Name == "John Doe");

					Assert.IsTrue(person.Groups != null);
					Assert.IsTrue(person.Groups.First().Group != null);
					Assert.AreEqual("Group", person.Groups.First().Group.Name);
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

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						var person = new PersonEntity { Name = "John Doe" };
						var group = new GroupEntity { Description = "For those who are epic and code.", Name = "Epic Coders" };
						person.Groups.Add(new GroupMemberEntity { Role = "Leader", Group = group });
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

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new PersonEntity { Name = "John Doe", Address = address });
						database.Addresses.Add(address);

						var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
						database.Addresses.Add(address2);
						address2.People.Add(new PersonEntity { Name = "Jane Doe", Address = address2 });

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
		public void RemoveEntityById()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
						Assert.AreEqual(0, database.Addresses.Count());

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count());

						var address = database.Addresses.First();
						database.Addresses.Remove(address.Id);
						Assert.AreEqual(1, database.Addresses.Count());

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
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

					database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
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
					Assert.AreEqual(1, database.Addresses.Count(x => x.IsDeleted));
					Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
				}
			});
		}

		[TestMethod]
		public void RemoveEntityDeleteCascade()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					GroupEntity group;

					using (var database = provider.GetDatabase())
					{
						database.Options.PermanentSyncEntityDeletions = true;

						Console.WriteLine(database.GetType().Name);

						group = GroupFactory.Get();
						database.Groups.Add(group);

						var address = AddressFactory.Get(null, "123 Main");
						var person = PersonFactory.Get(null, "John", address);
						var member = GroupMemberFactory.Get(group, person);
						database.GroupMembers.Add(member);

						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.People.Count());
						Assert.AreEqual(1, database.Groups.Count());
						Assert.AreEqual(1, database.GroupMembers.Count());

						database.Groups.Remove(group.Id);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.People.Count());
						Assert.AreEqual(0, database.Groups.Count());
						Assert.AreEqual(0, database.GroupMembers.Count());
					}
				});
		}

		[TestMethod]
		public void RemoveEntityDeleteSetNull()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					PetEntity pet;
					PetTypeEntity type;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = AddressFactory.Get(null, "123 Main");
						var person = PersonFactory.Get(null, "John", address);
						database.People.Add(person);
						database.SaveChanges();

						type = PetTypeFactory.Get();
						pet = PetFactory.Get(null, person);
						pet.Type = type;
						database.Pets.Add(pet);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.People.Count());
						Assert.AreEqual(1, database.Pets.Count());
						Assert.AreEqual(1, database.PetTypes.Count());

						database.Options.PermanentSyncEntityDeletions = true;
						var petTypes = database.PetTypes.FirstOrDefault();
						database.PetTypes.Remove(petTypes);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.People.Count());
						Assert.AreEqual(1, database.Pets.Count());
						Assert.AreEqual(0, database.PetTypes.Count());
						
						var actual = database.Pets.First();
						Assert.AreEqual(pet.Id, actual.Id);
						Assert.AreEqual(null, actual.TypeId);
					}
				});
		}

		[TestMethod]
		public void RemoveEntityDeleteHard()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						database.Options.PermanentSyncEntityDeletions = true;

						Console.WriteLine(database.GetType().Name);

						var tracker = new CollectionChangeTracker();
						database.CollectionChanged += (sender, args) => tracker.Update(args);
						database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						tracker.Reset();
						var address = database.Addresses.First();
						database.Addresses.Remove(address);
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						database.SaveChanges();
						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(1, tracker.Removed.Count);
						Assert.AreEqual(address, tracker.Removed[0]);
					}
				});
		}

		[TestMethod]
		public void RemoveEntityDeleteSoft()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						database.Options.PermanentSyncEntityDeletions = false;

						Console.WriteLine(database.GetType().Name);

						var tracker = new CollectionChangeTracker();
						database.CollectionChanged += (sender, args) => tracker.Update(args);
						database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						tracker.Reset();
						var address = database.Addresses.First();
						database.Addresses.Remove(address);
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);

						database.SaveChanges();
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.Count(x => x.IsDeleted));
						Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
						Assert.AreEqual(0, tracker.Added.Count);
						Assert.AreEqual(0, tracker.Removed.Count);
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
						database.Options.PermanentSyncEntityDeletions = true;

						Console.WriteLine(database.GetType().Name);

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new PersonEntity { Name = "John Doe" });
						database.Addresses.Add(address);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(1, database.People.Count());

						database.Addresses.Remove(address);

						// ReSharper disable once AccessToDisposedClosure
						TestHelper.ExpectedException<UpdateException>(() => database.SaveChanges(),
							"SQLite Error 19: 'FOREIGN KEY constraint failed'.",
							"The DELETE statement conflicted with the REFERENCE constraint \"FK_People_Addresses_AddressId\". The conflict occurred in database \"Speedy\", table \"dbo.People\", column 'AddressId'.",
							"The DELETE statement conflicted with the REFERENCE constraint.");
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
						database.Options.PermanentSyncEntityDeletions = true;
						Console.WriteLine(database.GetType().Name);

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						address.People.Add(new PersonEntity { Name = "John Doe" });
						database.Addresses.Add(address);

						Assert.AreEqual(0, database.Addresses.Count());
						Assert.AreEqual(0, database.People.Count());

						database.SaveChanges();

						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Addresses.First().People.Count);
						Assert.AreEqual(1, database.People.Count());

						database.People.Remove(address.People.First());
						database.SaveChanges();

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

						var pepper = new FoodEntity { Name = "Pepper" };
						var salt = new FoodEntity { Name = "Salt" };

						var steak = new FoodEntity
						{
							Name = "Steak",
							ChildRelationships = new[]
							{
								new FoodRelationshipEntity { Child = pepper, Quantity = 1 },
								new FoodRelationshipEntity { Child = salt, Quantity = 1 }
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

						var expected = new PersonEntity { Name = "Foo", Address = NewAddress("Bar") };
						database.People.Add(expected);
						database.SaveChanges();

						expected = new PersonEntity { Name = "Foo", Address = NewAddress("Bar") };
						database.People.Add(expected);

						// ReSharper disable once AccessToDisposedClosure
						TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
							"SQLite Error 19: 'UNIQUE constraint failed: People.Name'.",
							"Cannot insert duplicate key row in object 'dbo.People' with unique index 'IX_People_Name'. The duplicate key value is (Foo).",
							"PersonEntity: Cannot insert duplicate row. The duplicate key value is (Foo).");
					}
				});
		}

		[TestMethod]
		public void UnmodifiableEntityShouldNotAllowSave()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var logEvent = new LogEventEntity { Id = "Log1", Message = "Hello World" };
						database.LogEvents.Add(logEvent);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.LogEvents.Count());

						var entity = database.LogEvents.First();
						entity.Message = "Test";
						var count = database.SaveChanges();
						Assert.AreEqual(0, count);

						entity = database.LogEvents.First();
						Assert.AreEqual("Hello World", entity.Message);
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.LogEvents.Count());

						var entity = database.LogEvents.First();
						Assert.AreEqual("Hello World", entity.Message);
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

						var expected = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

						database.Addresses.Add(expected);
						var actual = database.Addresses.FirstOrDefault();
						Assert.IsNull(actual);
						database.SaveChanges();

						actual = database.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));

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

					database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
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

					database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
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

		[TestMethod]
		public void VirtualPropertyShouldNotSerialize()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new AddressEntity
						{
							City = "City",
							Line1 = "Line1",
							Line2 = "Line2",
							SyncId = Guid.Parse("513B9CF1-7596-4E2E-888D-835622A3FB2B"),
							Postal = "29640",
							State = "SC"
						};

						database.Addresses.Add(address);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var entity = database.Addresses.First();
						// Easier to assert on, over write automated values.
						entity.CreatedOn = new DateTime(2017, 01, 01, 01, 02, 03);
						entity.ModifiedOn = new DateTime(2017, 02, 02, 01, 02, 03);

						var actual = entity.ToSyncObject();
						var expect = "{\"$id\":\"1\",\"City\":\"City\",\"CreatedOn\":\"2017-01-01T01:02:03\",\"Id\":1,\"IsDeleted\":false,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"ModifiedOn\":\"2017-02-02T01:02:03\",\"Postal\":\"29640\",\"State\":\"SC\",\"SyncId\":\"513b9cf1-7596-4e2e-888d-835622a3fb2b\"}";

						actual.Data.Dump();

						Assert.AreEqual(expect, actual.Data);
					}
				});
		}

		private static AddressEntity NewAddress(string line1, string line2 = "")
		{
			return new AddressEntity { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		#endregion
	}
}