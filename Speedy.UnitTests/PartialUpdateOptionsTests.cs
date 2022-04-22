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
		public void Create()
		{
			var expected = new PartialUpdateOptions<MyClass>();
			var actual = PartialUpdateOptions.Create(typeof(MyClass));
			TestHelper.AreEqual(expected, actual);

			var instance1 = expected.GetInstance();
			var instance2 = actual.GetInstance();
			TestHelper.AreEqual(instance1, instance2);
			TestHelper.AreEqual(instance1.GetType(), instance2.GetType());
		}

		[TestMethod]
		public void PropertyValidationForMinMaxRangeForNumber()
		{
			var options = new PartialUpdateOptions<MyClass>();
			options.Validator
				.Property(x => x.Age)
				.HasMinMaxRange(10, 20, "Age must be between 10 and 20.");

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
			options.Validator
				.Property(x => x.Name)
				.HasMinMaxRange(10, 12, "Name must be between 10 and 12.");

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