#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class DatabaseTests
	{
		#region Methods

		[TestMethod]
		public void AddEntity()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
				{
					Console.WriteLine(context.GetType().Name);

					var expected = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };

					using (context)
					{
						context.Addresses.Add(expected);
						var actual = context.Addresses.FirstOrDefault();
						Assert.IsNull(actual);

						context.SaveChanges();

						actual = context.Addresses.FirstOrDefault();
						Assert.IsNotNull(actual);
						Assert.AreNotEqual(0, actual.Id);
						TestHelper.AreEqual(expected, actual);
					}
				}
			});
		}

		[TestMethod]
		public void AddEntityWithInvalidProperty()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
				{
					Console.WriteLine(context.GetType().Name);

					context.Addresses.Add(new Address { City = "City", Line2 = "Line2", Postal = "Postal", State = "State" });
					var test = context.Addresses.ToList();
					Assert.AreEqual(0, test.Count);

					// ReSharper disable once AccessToDisposedClosure
					TestHelper.ExpectedException<Exception>(() => context.SaveChanges(), "Address: The Line1 field is required.");
				}
			});
		}

		[TestMethod]
		public void AddMultipleEntitiesUsingRelationship()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
		public void AddSingleEntity()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
					TestHelper.ExpectedException<Exception>(() => context.SaveChanges(), "Address: The Line1 field is required.");
				}
			});
		}

		[TestMethod]
		public void AddSingleEntityUsingRelationship()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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

		[TestInitialize]
		public void Initialize()
		{
			TestHelper.Initialize();
		}

		[TestMethod]
		public void OptionalDirectRelationship()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
				using (var context = provider.CreateContext())
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

			providers.ForEach(provider =>
			{
				using (var context = provider.CreateContext())
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
		public void RelationshipCollectionsShouldFilter()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
				using (var context = provider.CreateContext())
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
				using (var context = provider.CreateContext())
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
		public void RemoveSingleEntityRelationship()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
		public void UpdateEntityCreatedOnShouldReset()
		{
			TestHelper.GetDataContexts().ForEach(context =>
			{
				using (context)
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
				using (var context = provider.CreateContext())
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
				using (var context = provider.CreateContext())
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
				using (var context = provider.CreateContext())
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
				using (var context = provider.CreateContext())
				{
					Console.WriteLine(context.GetType().Name);

					var address1 = context.Addresses.First();
					Assert.AreEqual("Line One", address1.Line1);
				}
			});
		}

		#endregion
	}
}