#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.WebApi;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Storage;
using Speedy.UnitTests.Factories;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	/// <summary>
	/// Add test for Bulk Remove with default of true (x => true), not working
	/// </summary>
	[TestClass]
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public class DatabaseTests
	{
		#region Methods

		[TestMethod]
		public void AddEntity()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void AddEntityViaSubRelationships()
		{
			TestHelper.GetDataContexts(initialized: false)
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
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
						var expected = new AccountEntity { Name = "Foo", Address = address };
						database.Accounts.Add(expected);
						database.SaveChanges();

						var pet = new PetEntity { Name = "Max", Owner = expected, Type = new PetTypeEntity { Id = "Dog", Type = "Boston" } };
						database.Pets.Add(pet);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var owner = database.Accounts.Including(x => x.Pets).First();
						var actual = owner.Pets.First();

						Assert.AreEqual("Max", actual.Name);
						Assert.AreEqual(owner.Id, actual.OwnerId);
					}
				});
		}

		[TestMethod]
		public void AddEntityWithCompositeKeyShouldNotAllowDuplicatesAssignedUsingId()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					var expected = new AccountEntity { Name = "Foo", Address = address };
					database.Accounts.Add(expected);
					database.SaveChanges();

					var pet = new PetEntity { Name = "Max", OwnerId = expected.Id, Type = new PetTypeEntity { Id = "Dog", Type = "Boston" } };
					database.Pets.Add(pet);
					database.SaveChanges();

					pet = new PetEntity { Name = "Max", OwnerId = expected.Id, Type = new PetTypeEntity { Id = "Dog", Type = "Boston" } };

					TestHelper.ExpectedException<InvalidOperationException>(() => database.Pets.Add(pet),
						"The instance of entity type 'PetEntity' cannot be tracked because another instance with the same key value");
				});
		}

		[TestMethod]
		public void AddEntityWithCompositeKeyShouldNotAllowDuplicatesAssignUsingType()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					var expected = new AccountEntity { Name = "Foo", Address = address };
					database.Accounts.Add(expected);
					database.SaveChanges();

					var pet = new PetEntity { Name = "Max", Owner = expected, Type = new PetTypeEntity { Id = "Dog", Type = "Boston" } };
					database.Pets.Add(pet);
					database.SaveChanges();

					pet = new PetEntity { Name = "Max", Owner = expected, Type = new PetTypeEntity { Id = "Dog", Type = "Boston" } };

					TestHelper.ExpectedException<InvalidOperationException>(() => database.Pets.Add(pet),
						"The instance of entity type 'PetEntity' cannot be tracked because another instance with the same key value");
				});
		}

		[TestMethod]
		public void AddEntityWithInvalidProperty()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					database.Addresses.Add(new AddressEntity { City = "City", Line2 = "Line2", Postal = "Postal", State = "State" });
					var test = database.Addresses.ToList();
					Assert.AreEqual(0, test.Count);

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
						"SQLite Error 19: 'NOT NULL constraint failed: Addresses.AddressLineOne'.",
						"Cannot insert the value NULL into column 'AddressLineOne', table 'Speedy.dbo.Addresses'; column does not allow nulls. INSERT fails.",
						"AddressEntity: The Line1 field is required.");
				});
		}

		[TestMethod]
		public void AddEntityWithMultipleDefaultKeys()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					database.LogEvents.Add(EntityFactory.GetLogEvent("foo"));
					database.LogEvents.Add(EntityFactory.GetLogEvent("bar"));
					database.LogEvents.Add(EntityFactory.GetLogEvent("foo"));
				});
		}

		[TestMethod]
		public void AddEntityWithoutMaintainDatesDatabaseOption()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void AddEntityWithUnmaintainEntityDatabaseOption()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void AddMultipleEntitiesUsingRelationship()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.Accounts.Add(new AccountEntity { Name = "John Doe" });
					database.Addresses.Add(address);

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());

					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreNotEqual(0, database.Addresses.First().Id);
					Assert.AreEqual(1, database.Accounts.Count());
					Assert.AreNotEqual(0, database.Accounts.First().Id);
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreNotEqual(0, database.Addresses.First().Accounts.First().AddressId);
				});
		}

		[TestMethod]
		public void AddMultipleEntitiesUsingRelationshipWithSaves()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address);

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());
					database.SaveChanges();

					database.Accounts.Add(new AccountEntity { Name = "John Doe", AddressId = address.Id });

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());
					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreNotEqual(0, database.Addresses.First().Id);
					Assert.AreEqual(1, database.Accounts.Count());
					Assert.AreNotEqual(0, database.Accounts.First().Id);
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreNotEqual(0, database.Addresses.First().Accounts.First().AddressId);
				});
		}

		[TestMethod]
		public void AddNonModifiableEntity()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var expected = EntityFactory.GetLogEvent("The new log message that is really important.");
					database.LogEvents.Add(expected);

					var actual = database.LogEvents.FirstOrDefault();
					Assert.IsNull(actual);

					database.SaveChanges();

					actual = database.LogEvents.FirstOrDefault();
					Assert.IsNotNull(actual);
					Assert.AreNotEqual(0, actual.Id);
					Assert.AreNotEqual(default, actual.CreatedOn);
					TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));
				});
		}

		[TestMethod]
		public void AddSingleEntity()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void AddSingleEntityMissingRequiredProperty()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);
					database.Addresses.Add(new AddressEntity { Line1 = null, Line2 = string.Empty, City = "Easley", State = "SC", Postal = "29640" });
					TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
						"SQLite Error 19: 'NOT NULL constraint failed: Addresses.AddressLineOne'.",
						"Cannot insert the value NULL into column 'AddressLineOne', table 'Speedy.dbo.Addresses'; column does not allow nulls. INSERT fails.",
						"AddressEntity: The Line1 field is required.");
				});
		}

		[TestMethod]
		public void AddSingleEntityMissingRequiredRelationship()
		{
			// todo: finish this test
			//TestHelper.GetDataContexts(initialized: false)
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
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address);

					var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address2);

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());

					database.SaveChanges();

					Assert.AreEqual(2, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());

					address.Accounts.Add(new AccountEntity { Name = "John Doe" });
					Assert.AreEqual(1, address.Accounts.Count);

					database.SaveChanges();

					Assert.AreEqual(2, database.Addresses.Count());
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreEqual(1, database.Accounts.Count());
				});
		}

		[TestMethod]
		public void AddTwoEntities()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void BulkRemoveShouldDelete()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
					{
						var filters = new List<Expression<Func<AddressEntity, bool>>>
						{
							x => x.SyncId != Guid.Empty,
							x => !x.IsDeleted,
							x => x.CreatedOn > DateTime.MinValue && x.ModifiedOn < DateTime.MaxValue,
							x => x.Id >= byte.MinValue && x.Id <= byte.MaxValue,
							x => x.Id >= ushort.MinValue && x.Id <= ushort.MaxValue,
							x => x.Id >= short.MinValue && x.Id <= short.MaxValue,
							x => x.Id >= int.MinValue && x.Id <= int.MaxValue,
							x => x.Id >= uint.MinValue && x.Id <= uint.MaxValue,
							x => x.Id >= long.MinValue && x.Id <= long.MaxValue
						};

						filters.ForEach(filter =>
						{
							using (var database = provider.GetDatabase())
							{
								Console.WriteLine(database.GetType().Name + ": " + filter);

								for (var i = 0; i < 10; i++)
								{
									var address = new AddressEntity { City = $"City{i}", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
									database.Addresses.Add(address);
								}

								database.SaveChanges();

								Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
								Assert.AreEqual(10, database.Addresses.Count(x => !x.IsDeleted));

								var count = database.Addresses.BulkRemove(filter);

								Assert.AreEqual(10, count);
							}

							using (var database = provider.GetDatabase())
							{
								Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
								Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
							}
						});
					}
				);
		}

		[TestMethod]
		public void BulkRemoveShouldDeletePartial()
		{
			var address1Guid = Guid.Parse("5C5C5A2E-29CB-497D-A5CB-7027A66DD6B8");

			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
					{
						var filters = new Dictionary<Expression<Func<AddressEntity, bool>>, int>
						{
							//{ x => x.Id >= 5, 4 },
							{ x => x.SyncId == address1Guid, 9 }
						};

						filters.ForEach(filter =>
						{
							using (var database = provider.GetDatabase())
							{
								Console.WriteLine(database.GetType().Name);
								database.Addresses.BulkRemove(x => x.Id > 0);

								var addresses = new[]
								{
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("D2C661DB-83A1-4AC3-8E9B-FB93ABC23549") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("4D015D1B-7F8A-4CD8-B517-813A0CB62780") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("E1D35855-7575-4060-85F5-D1ADE014B7EE") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("93BDB84D-A5AE-4BED-974C-2DDDFC79618D") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("5C5C5A2E-29CB-497D-A5CB-7027A66DD6B8") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("D183C149-D66C-4BB6-9626-85AF9C9712D5") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("08F42C69-E583-4073-8126-B75740A340AF") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("6361D354-5364-4D04-837A-A1D2694ABD22") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("DF12B490-31DE-4BEF-ABC4-96DAA4286536") },
									new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State", SyncId = Guid.Parse("A5DF65DC-1AA5-4256-9C05-E8417A6D483F") }
								};

								foreach (var address in addresses)
								{
									database.Addresses.Add(address);
								}

								database.SaveChanges();

								Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
								Assert.AreEqual(10, database.Addresses.Count(x => !x.IsDeleted));

								var count = database.Addresses.BulkRemove(filter.Key);

								Assert.AreEqual(10 - filter.Value, count);
							}

							using (var database = provider.GetDatabase())
							{
								Assert.AreEqual(0, database.Addresses.Count(x => x.IsDeleted));
								Assert.AreEqual(filter.Value, database.Addresses.Count(x => !x.IsDeleted));
							}
						});
					}
				);
		}

		[TestMethod]
		public void BulkUpdateShouldUpdate()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
					{
						using var database = provider.GetDatabase();
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
				);
		}

		[TestMethod]
		public void DateTimeShouldUseUtc()
		{
			var dateTime = new DateTime(2020, 05, 14, 08, 54, 12, DateTimeKind.Unspecified);
			var actual = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			Assert.AreEqual(dateTime.Hour, actual.Hour);

			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					var testCases = new List<DateTime>
					{
						new DateTime(2020, 05, 14, 08, 23, 45, DateTimeKind.Local),
						new DateTime(2020, 05, 14, 08, 23, 45, DateTimeKind.Unspecified),
						new DateTime(2020, 05, 14, 12, 23, 45, DateTimeKind.Utc)
					};

					foreach (var testCase in testCases)
					{
						var expected = new LogEventEntity { AcknowledgedOn = testCase, LoggedOn = testCase, Message = "Hello World" };

						using (var database = provider.GetDatabase())
						{
							if (!(database is EntityFrameworkDatabase))
							{
								// Test is only for entity framework databases
								return;
							}

							Console.WriteLine(database.GetType().Name + " : " + testCase.ToString());
							database.LogEvents.Add(expected);
							database.SaveChanges();
						}

						using (var database = provider.GetDatabase())
						{
							var actual = database.LogEvents.FirstOrDefault(x => x.Id == expected.Id);
							Assert.IsNotNull(actual);
							Assert.AreEqual(DateTimeKind.Utc, actual.AcknowledgedOn.Value.Kind);
							Assert.AreEqual(12, actual.AcknowledgedOn.Value.Hour);
							Assert.AreEqual(DateTimeKind.Utc, actual.LoggedOn.Kind);
							Assert.AreEqual(12, actual.LoggedOn.Hour);
						}
					}
				});
		}
		
		[TestMethod]
		public void DateTimeMaxShouldUseUtc()
		{
			var dateTime = new DateTime(2020, 05, 14, 08, 54, 12, DateTimeKind.Unspecified);
			var actual = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			Assert.AreEqual(dateTime.Hour, actual.Hour);

			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					var testCases = new List<DateTime>
					{
						new DateTime(DateTimeExtensions.MaxDateTimeTicks, DateTimeKind.Local),
						new DateTime(DateTimeExtensions.MaxDateTimeTicks, DateTimeKind.Unspecified),
						new DateTime(DateTimeExtensions.MaxDateTimeTicks, DateTimeKind.Utc)
					};

					foreach (var testCase in testCases)
					{
						var expected = new LogEventEntity { AcknowledgedOn = testCase, LoggedOn = testCase, Message = "Hello World" };

						using (var database = provider.GetDatabase())
						{
							if (!(database is EntityFrameworkDatabase))
							{
								// Test is only for entity framework databases
								return;
							}

							Console.WriteLine(database.GetType().Name + " : " + testCase.ToString());
							database.LogEvents.Add(expected);
							database.SaveChanges();
						}

						using (var database = provider.GetDatabase())
						{
							var actual = database.LogEvents.FirstOrDefault(x => x.Id == expected.Id);
							Assert.IsNotNull(actual);
							
							Assert.AreEqual(DateTimeKind.Utc, actual.AcknowledgedOn.Value.Kind);
							Assert.AreEqual(DateTimeExtensions.MaxDateTimeTicks, actual.AcknowledgedOn.Value.Ticks);
							Assert.AreEqual(DateTime.MaxValue, actual.AcknowledgedOn.Value);

							Assert.AreEqual(DateTimeKind.Utc, actual.LoggedOn.Kind);
							Assert.AreEqual(DateTimeExtensions.MaxDateTimeTicks, actual.LoggedOn.Ticks);
							Assert.AreEqual(DateTime.MaxValue, actual.LoggedOn);
						}
					}
				});
		}

		[TestMethod]
		public void DateTimeMinShouldUseUtc()
		{
			var dateTime = new DateTime(2020, 05, 14, 08, 54, 12, DateTimeKind.Unspecified);
			var actual = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			Assert.AreEqual(dateTime.Hour, actual.Hour);

			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					var testCases = new List<DateTime>
					{
						new DateTime(DateTimeExtensions.MinDateTimeTicks, DateTimeKind.Local),
						new DateTime(DateTimeExtensions.MinDateTimeTicks, DateTimeKind.Unspecified),
						new DateTime(DateTimeExtensions.MinDateTimeTicks, DateTimeKind.Utc)
					};

					foreach (var testCase in testCases)
					{
						var expected = new LogEventEntity { AcknowledgedOn = testCase, LoggedOn = testCase, Message = "Hello World" };

						using (var database = provider.GetDatabase())
						{
							if (!(database is EntityFrameworkDatabase))
							{
								// Test is only for entity framework databases
								return;
							}

							Console.WriteLine(database.GetType().Name + " : " + testCase.ToString());
							database.LogEvents.Add(expected);
							database.SaveChanges();
						}

						using (var database = provider.GetDatabase())
						{
							var actual = database.LogEvents.FirstOrDefault(x => x.Id == expected.Id);
							Assert.IsNotNull(actual);
							
							Assert.AreEqual(DateTimeKind.Utc, actual.AcknowledgedOn.Value.Kind);
							Assert.AreEqual(DateTimeExtensions.MinDateTimeTicks, actual.AcknowledgedOn.Value.Ticks);
							Assert.AreEqual(DateTime.MinValue, actual.AcknowledgedOn.Value);

							Assert.AreEqual(DateTimeKind.Utc, actual.LoggedOn.Kind);
							Assert.AreEqual(DateTimeExtensions.MinDateTimeTicks, actual.LoggedOn.Ticks);
							Assert.AreEqual(DateTime.MinValue, actual.LoggedOn);
						}
					}
				});
		}

		[TestMethod]
		public void DiscardChanges()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void EntitiesWithInterfaceShouldStillSerialize()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					var expected = EntityFactory.GetLogEvent("This is a test.");

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

		[TestMethod]
		public void IndexesWithMultipleColumnsAndUniqueShouldNotException()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					var person1 = EntityFactory.GetAccount(x => x.Name = "John");
					var person2 = EntityFactory.GetAccount(x => x.Name = "Jane");
					database.Accounts.Add(person1);
					database.Accounts.Add(person2);
					database.SaveChanges();

					database.Pets.Add(new PetEntity { Name = "Spot", Owner = person1 });
					database.Pets.Add(new PetEntity { Name = "Spot", Owner = person2 });
					database.SaveChanges();
				});
		}

		[TestMethod]
		public void IndexesWithUniqueShouldException()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					database.Food.Add(EntityFactory.GetFood(x => x.Name = "Bread"));
					database.Food.Add(EntityFactory.GetFood(x => x.Name = "Bread"));

					TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
						"Cannot insert duplicate key row in object 'dbo.Foods' with unique index 'IX_Foods_Name'. The duplicate key value is (Bread)",
						"SQLite Error 19: 'UNIQUE constraint failed: Foods.Name'",
						"IX_Foods_Name: Cannot insert duplicate row. The duplicate key value is (Bread)."
					);
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
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address1 = new AddressEntity { City = "City", Line1 = "Line", Line2 = "Line", Postal = "Postal", State = "State" };
					var address2 = new AddressEntity { City = "City2", Line1 = "Line2", Line2 = "Line2", Postal = "Postal2", State = "State2" };
					address1.LinkedAddress = address2;
					database.Addresses.Add(address1);
					database.SaveChanges();

					var expected = new[] { address2, address1 };
					var actual = database.Addresses.ToArray();
					TestHelper.AreEqual(expected, actual, nameof(AddressEntity.CreatedOn), nameof(AddressEntity.ModifiedOn));
				});
		}

		[TestMethod]
		public void QueryUsingInclude()
		{
			var providers = TestHelper.GetDataContexts(initialized: false).ToList();

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
				address.Accounts.Add(new AccountEntity { Name = "John Doe", Address = address });
				database.Addresses.Add(address);

				var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
				database.Addresses.Add(address2);
				address2.Accounts.Add(new AccountEntity { Name = "Jane Doe", Address = address2 });

				Assert.AreEqual(0, database.Addresses.Count());
				Assert.AreEqual(0, database.Accounts.Count());

				database.SaveChanges();

				Assert.AreEqual(2, database.Addresses.Count());
				Assert.AreEqual(2, database.Accounts.Count());
				Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
			});

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var person = database.Accounts
					.Include(x => x.Address)
					.First(x => x.Name == "John Doe");

				Assert.IsTrue(person.Address != null);
			});
		}

		[TestMethod]
		public void QueryUsingIncluding()
		{
			var providers = TestHelper.GetDataContexts(initialized: false).ToList();

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
				address.Accounts.Add(new AccountEntity { Name = "John Doe", Address = address });
				database.Addresses.Add(address);

				var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
				database.Addresses.Add(address2);
				address2.Accounts.Add(new AccountEntity { Name = "Jane Doe", Address = address2 });

				Assert.AreEqual(0, database.Addresses.Count());
				Assert.AreEqual(0, database.Accounts.Count());

				database.SaveChanges();

				Assert.AreEqual(2, database.Addresses.Count());
				Assert.AreEqual(2, database.Accounts.Count());
				Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
			});

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var person = database.Accounts
					.Including(x => x.Address, x => x.Groups)
					.First(x => x.Name == "John Doe");

				Assert.IsTrue(person.Address != null);
				Assert.IsTrue(person.Groups != null);
			});
		}

		[TestMethod]
		public void QueryUsingRecursiveRelationshipsUsingExtensionMethods()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					var length = 20;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 0; i < length; i++)
						{
							var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
							address.Accounts.Add(new AccountEntity { Name = "John Doe #" + i, Address = address });
							database.Addresses.Add(address);
						}

						database.SaveChanges();
						Assert.AreEqual(length, database.Addresses.Count());
						Assert.AreEqual(length, database.Accounts.Count());
					}

					using (var database = provider.GetDatabase())
					{
						var address = database.Addresses
							.Where(x => x.Accounts.Any())
							.GetRandomItem();

						Assert.IsNotNull(address);
					}
				});
		}

		[TestMethod]
		public void QueryUsingThenInclude()
		{
			var providers = TestHelper.GetDataContexts(initialized: false).ToList();

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
				var person = new AccountEntity { Name = "John Doe", Address = address };
				person.Groups.Add(new GroupMemberEntity { Group = new GroupEntity { Name = "Group", Description = "Description" }, Role = "Role" });
				address.Accounts.Add(person);
				database.Addresses.Add(address);

				Assert.AreEqual(0, database.Addresses.Count());
				Assert.AreEqual(0, database.Accounts.Count());
				Assert.AreEqual(0, database.Accounts.Count());

				database.SaveChanges();

				Assert.AreEqual(1, database.Addresses.Count());
				Assert.AreEqual(1, database.Accounts.Count());
				Assert.AreEqual(1, database.Groups.Count());
				Assert.AreEqual(1, database.GroupMembers.Count());
			});

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var person = database.Accounts
					.Include(x => x.Pets)
					.Include(x => x.Groups)
					.ThenInclude(x => x.Group)
					.First(x => x.Name == "John Doe");

				Assert.IsTrue(person.Groups != null);
				Assert.IsTrue(person.Groups.First().Group != null);
				Assert.AreEqual("Group", person.Groups.First().Group.Name);
			});
		}

		[TestMethod]
		public void RelationshipCollectionsCountShouldBeCorrect()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					var person = new AccountEntity { Name = "John Doe" };
					var group = new GroupEntity { Description = "For those who are epic and code.", Name = "Epic Coders" };
					person.Groups.Add(new GroupMemberEntity { Role = "Leader", Group = group });
					address.Accounts.Add(person);
					database.Addresses.Add(address);

					Assert.AreEqual(1, address.Accounts.Count);
					Assert.AreEqual(1, person.Groups.Count);
					Assert.AreEqual(1, group.Members.Count);
					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());
					Assert.AreEqual(0, database.GroupMembers.Count());
					Assert.AreEqual(0, database.Groups.Count());

					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreNotEqual(0, database.Addresses.First().Id);
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreEqual(1, database.Accounts.Count());
					Assert.AreNotEqual(0, database.Accounts.First().Id);
					Assert.AreEqual(1, database.Accounts.First().Groups.Count);
					Assert.AreEqual(1, database.GroupMembers.Count());
					Assert.AreNotEqual(0, database.GroupMembers.First().Id);
					Assert.AreEqual(1, database.Groups.Count());
					Assert.AreNotEqual(0, database.Groups.First().Id);
				});
		}

		[TestMethod]
		public void RelationshipCollectionsShouldFilter()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.Accounts.Add(new AccountEntity { Name = "John Doe", Address = address });
					database.Addresses.Add(address);

					var address2 = new AddressEntity { City = "City", Line1 = "Line2", Line2 = "Line2", Postal = "Postal", State = "State" };
					database.Addresses.Add(address2);
					address2.Accounts.Add(new AccountEntity { Name = "Jane Doe", Address = address2 });

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());

					database.SaveChanges();

					Assert.AreEqual(2, database.Addresses.Count());
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreEqual(2, database.Accounts.Count());
				});
		}

		[TestMethod]
		public void RemoveEntityById()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void RemoveEntityByIdWithDetachedEntity()
		{
			var providers = TestHelper.GetDataContexts(initialized: false).ToList();

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.Addresses.Add(new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" });
				Assert.AreEqual(0, database.Addresses.Count());

				database.SaveChanges();
				Assert.AreEqual(1, database.Addresses.Count());
			});

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var address = database.Addresses.First();
				database.Addresses.Remove(address.Id);
				Assert.AreEqual(1, database.Addresses.Count());

				database.SaveChanges();
				Assert.AreEqual(1, database.Addresses.Count(x => x.IsDeleted));
				Assert.AreEqual(0, database.Addresses.Count(x => !x.IsDeleted));
			});
		}

		[TestMethod]
		public void RemoveEntityDeleteCascade()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					GroupEntity group;

					using (var database = provider.GetDatabase())
					{
						database.Options.PermanentSyncEntityDeletions = true;

						Console.WriteLine(database.GetType().Name);

						group = EntityFactory.GetGroup();
						database.Groups.Add(group);

						var address = EntityFactory.GetAddress(null, "123 Main");
						var person = EntityFactory.GetAccount(null, "John", address);
						var member = EntityFactory.GetGroupMember(group, person);
						database.GroupMembers.Add(member);

						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Accounts.Count());
						Assert.AreEqual(1, database.Groups.Count());
						Assert.AreEqual(1, database.GroupMembers.Count());

						database.Groups.Remove(group.Id);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Accounts.Count());
						Assert.AreEqual(0, database.Groups.Count());
						Assert.AreEqual(0, database.GroupMembers.Count());
					}
				});
		}

		[TestMethod]
		public void RemoveEntityDeleteHard()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void RemoveEntityDeleteSetNull()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					PetEntity pet;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var address = EntityFactory.GetAddress(null, "123 Main");
						var person = EntityFactory.GetAccount(null, "John", address);
						database.Accounts.Add(person);
						database.SaveChanges();

						var type = EntityFactory.GetPetType();
						pet = EntityFactory.GetPet(null, person);
						pet.Type = type;
						database.Pets.Add(pet);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(1, database.Addresses.Count());
						Assert.AreEqual(1, database.Accounts.Count());
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
						Assert.AreEqual(1, database.Accounts.Count());
						Assert.AreEqual(1, database.Pets.Count());
						Assert.AreEqual(0, database.PetTypes.Count());

						var actual = database.Pets.First();
						Assert.AreEqual(pet.Id, actual.Id);
						Assert.AreEqual(null, actual.TypeId);
					}
				});
		}

		[TestMethod]
		public void RemoveEntityDeleteSoft()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void RemoveSingleEntityDependantRelationship()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					database.Options.PermanentSyncEntityDeletions = true;

					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.Accounts.Add(new AccountEntity { Name = "John Doe" });
					database.Addresses.Add(address);

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());

					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreEqual(1, database.Accounts.Count());

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<InvalidOperationException>(() => database.Addresses.Remove(address),
						"The association between entity types 'AddressEntity' and 'AccountEntity' has been severed but the relationship is either marked as 'Required' or is implicitly required because the foreign key is not nullable."
					);
				});
		}

		[TestMethod]
		public void RemoveSingleEntityRelationship()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					database.Options.PermanentSyncEntityDeletions = true;
					Console.WriteLine(database.GetType().Name);

					var address = new AddressEntity { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
					address.Accounts.Add(new AccountEntity { Name = "John Doe" });
					database.Addresses.Add(address);

					Assert.AreEqual(0, database.Addresses.Count());
					Assert.AreEqual(0, database.Accounts.Count());

					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreEqual(1, database.Addresses.First().Accounts.Count);
					Assert.AreEqual(1, database.Accounts.Count());

					database.Accounts.Remove(address.Accounts.First());
					database.SaveChanges();

					Assert.AreEqual(1, database.Addresses.Count());
					Assert.AreEqual(0, database.Addresses.First().Accounts.Count);
					Assert.AreEqual(0, database.Accounts.Count());
				});
		}

		[TestMethod]
		public void SubRelationships()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void UniqueConstraintsForString()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					var expected = new AccountEntity { Name = "Foo", Address = NewAddress("Bar") };
					database.Accounts.Add(expected);
					database.SaveChanges();

					expected = new AccountEntity { Name = "Foo", Address = NewAddress("Bar") };
					database.Accounts.Add(expected);

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
						"SQLite Error 19: 'UNIQUE constraint failed: Accounts.AccountName'.",
						"Cannot insert duplicate key row in object 'dbo.Accounts' with unique index 'IX_Accounts_Name'. The duplicate key value is (Foo).",
						"IX_Accounts_Name: Cannot insert duplicate row. The duplicate key value is (Foo).");
				});
		}
		
		[TestMethod]
		public void UniqueConstraintsForStringAllowNullButShouldStillEnforceUnique()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					// Both being null should be fine
					var expected1 = new AccountEntity { Name = "Foo1", Address = NewAddress("Main") };
					var expected2 = new AccountEntity { Name = "Foo2", Address = NewAddress("Main") };

					database.Accounts.Add(expected1);
					database.Accounts.Add(expected2);
					database.SaveChanges();

					// Both being unique should be fine
					expected1.Nickname = "Bar1";
					database.SaveChanges();
					expected2.Nickname = "Bar2";
					database.SaveChanges();
					
					// Now if both are the same it should exception
					expected1.Nickname = "Bar";
					database.SaveChanges();
					expected2.Nickname = "Bar";

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
						"SQLite Error 19: 'UNIQUE constraint failed: Accounts.AccountNickname'.",
						"Cannot insert duplicate key row in object 'dbo.Accounts' with unique index 'IX_Accounts_Nickname'. The duplicate key value is (Bar).",
						"IX_Accounts_Nickname: Cannot insert duplicate row. The duplicate key value is (Bar).");
				});
		}

		[TestMethod]
		public void UniqueConstraintsForCompositeKeyWithStringThatAllowNullButShouldStillEnforceUnique()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					// Both being null should be fine
					var address = NewAddress("Main");
					var expected1 = new AccountEntity { Name = "Foo1" };
					var expected2 = new AccountEntity { Name = "Foo2" };
					address.Accounts.Add(expected1);
					address.Accounts.Add(expected2);

					database.Addresses.Add(address);
					database.SaveChanges();

					// Both being unique should be fine
					expected1.ExternalId = "EID-1";
					expected2.ExternalId = "EID-2";
					database.SaveChanges();
					
					// Now if both are the same it should exception
					expected1.ExternalId = "EID";
					database.SaveChanges();
					expected2.ExternalId = "EID";

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
						"SQLite Error 19: 'UNIQUE constraint failed: Accounts.AccountAddressId, Accounts.AccountExternalId'.",
						"Cannot insert duplicate key row in object 'dbo.Accounts' with unique index 'IX_Accounts_AddressId_ExternalId'. The duplicate key value is (1, EID).",
						"IX_Accounts_AddressId_ExternalId: Cannot insert duplicate row. The duplicate key value is (1,EID).");
				});
		}
		
		[TestMethod]
		public void UniqueConstraintsForStringAllowNullButShouldStillEnforceUniqueViaRelationship()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
					Console.WriteLine(database.GetType().Name);

					// Both being null should be fine
					var expected1 = new AccountEntity { Name = "Foo1", Address = NewAddress("Main") };
					var expected2 = new AccountEntity { Name = "Foo2", Address = NewAddress("Main") };

					var pet1 = new PetEntity { Name = "Spot", Owner = expected1 };
					var pet2 = new PetEntity { Name = "Fred", Owner = expected2 };

					database.Pets.Add(pet1);
					database.Pets.Add(pet2);
					database.SaveChanges();

					//// Both being unique should be fine
					//expected1.Nickname = "Bar1";
					//expected2.Nickname = "Bar2";
					//database.SaveChanges();
					
					//// Now if both are the same it should exception
					//expected1.Nickname = "Bar";
					//expected2.Nickname = "Bar";

					//// ReSharper disable once AccessToDisposedClosure
					//TestHelper.ExpectedException<Exception>(() => database.SaveChanges(),
					//	"SQLite Error 19: 'UNIQUE constraint failed: Accounts.Nickname'.",
					//	"Cannot insert duplicate key row in object 'dbo.Accounts' with unique index 'IX_Accounts_Nickname'. The duplicate key value is (Bar).",
					//	"IX_Accounts_Nickname: Cannot insert duplicate row. The duplicate key value is (Bar).");
				});
		}

		[TestMethod]
		public void UnmodifiableEntityShouldNotAllowSave()
		{
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						var logEvent = EntityFactory.GetLogEvent("Hello World", LogLevel.Critical);
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
			TestHelper.GetDataContexts(initialized: false)
				.ForEach(provider =>
				{
					using var database = provider.GetDatabase();
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
				});
		}

		[TestMethod]
		public void UpdateEntityWithoutSave()
		{
			var providers = TestHelper.GetDataContexts(initialized: false).ToList();

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
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
			});

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var address1 = database.Addresses.First();
				Assert.AreEqual("Line1", address1.Line1);
			});
		}

		[TestMethod]
		public void UpdateEntityWithSave()
		{
			var providers = TestHelper.GetDataContexts(initialized: false).ToList();

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
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
			});

			providers.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				var address1 = database.Addresses.First();
				Assert.AreEqual("Line One", address1.Line1);
			});
		}

		[TestMethod]
		public void VirtualPropertyShouldNotSerialize()
		{
			TestHelper.GetDataContexts(initialized: false)
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