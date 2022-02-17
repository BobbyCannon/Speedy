#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Exceptions;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PartialUpdateOptionsTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void CreateInstance()
		{
			var options = new PartialUpdateOptions<MyClass>();
			var actual = options.GetInstance();
			Assert.AreEqual(typeof(MyClass), actual.GetType());
		}

		[TestMethod]
		public void PropertyValidationForMinMaxRangeForNumber()
		{
			var options = new PartialUpdateOptions<MyClass>();
			var property = options
				.Property(x => x.Age)
				.HasMinMaxRange(10, 20)
				.Throws("Age must be between 10 and 20.");

			Assert.AreEqual(1, options.Validations.Count);
			Assert.AreEqual("Age must be between 10 and 20.", property.Message);
			Assert.AreEqual(nameof(MyClass.Age), property.Name);
			Assert.AreEqual(false, property.Required);

			TestHelper.ExpectedException<ValidationException>(() => PartialUpdate.FromJson("{ \"age\": 9 }", options).Validate(), "Age must be between 10 and 20.");

			var update = PartialUpdate.FromJson("{ \"age\": 10 }", options);
			Assert.AreEqual(options, update.Options);
			update.Validate();
			var actual = (MyClass) update.GetInstance();
			Assert.AreEqual(10, actual.Age);

			update = PartialUpdate.FromJson("{ \"age\": 20 }", options);
			update.Validate();
			actual = (MyClass) update.GetInstance();
			Assert.AreEqual(20, actual.Age);

			TestHelper.ExpectedException<ValidationException>(() => PartialUpdate.FromJson("{ \"age\": 21 }", options).Validate(), "Age must be between 10 and 20.");
		}

		[TestMethod]
		public void PropertyValidationForMinMaxRangeForString()
		{
			var options = new PartialUpdateOptions<MyClass>();
			var property = options
				.Property(x => x.Name)
				.HasMinMaxRange(10, 12)
				.Throws("Name must be between 10 and 12.");

			Assert.AreEqual("Name must be between 10 and 12.", property.Message);
			Assert.AreEqual(nameof(MyClass.Name), property.Name);
			Assert.AreEqual(false, property.Required);

			TestHelper.ExpectedException<ValidationException>(() => PartialUpdate.FromJson("{ \"name\": \"123456789\" }", options).Validate(), "Name must be between 10 and 12.");

			var update = PartialUpdate.FromJson("{ \"name\": \"1234567890\" }", options);
			update.Validate();
			var actual = (MyClass) update.GetInstance();
			Assert.AreEqual("1234567890", actual.Name);

			update = PartialUpdate.FromJson("{ \"name\": \"123456789012\" }", options);
			update.Validate();
			actual = (MyClass) update.GetInstance();
			Assert.AreEqual("123456789012", actual.Name);

			TestHelper.ExpectedException<ValidationException>(() => PartialUpdate.FromJson("{ \"name\": \"123456789\" }", options).Validate(), "Name must be between 10 and 12.");
		}

		#endregion

		#region Classes

		public class MyClass : SyncModel<long>
		{
			#region Properties

			public int Age { get; set; }

			public override long Id { get; set; }

			public string Name { get; set; }

			#endregion
		}

		#endregion
	}
}