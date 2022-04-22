#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Exceptions;
using Speedy.Validation;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Validation
{
	[TestClass]
	public class ValidatorTests
	{
		#region Methods

		[TestMethod]
		public void EntityValidations()
		{
			var address = new AccountEntity();
			var validator = new Validator<AccountEntity>();

			validator.Property(x => x.Name)
				.HasMinMaxRange(1, 5, "The name must be between a minimum of 1 and a maximum length of 5.")
				.IsRequired("The name must be provided.");

			validator.Property(x => x.Id).HasMinMaxRange(1, int.MaxValue, "The Id must be set.");
			validator.Property(x => x.CreatedOn).IsRequired("The CreatedOn value must be provided.");
			validator.Property(x => x.AddressSyncId).IsRequired("The address sync ID is required.");

			var failedValidations = validator.Process(address).ToList();
			Assert.AreEqual(5, failedValidations.Count);
			Assert.AreEqual("The name must be between a minimum of 1 and a maximum length of 5.", failedValidations[0].Message);
			Assert.AreEqual("Name", failedValidations[0].Name);
			Assert.AreEqual("The name must be provided.", failedValidations[1].Message);
			Assert.AreEqual("Name", failedValidations[1].Name);
			Assert.AreEqual("The Id must be set.", failedValidations[2].Message);
			Assert.AreEqual("Id", failedValidations[2].Name);
			Assert.AreEqual("The CreatedOn value must be provided.", failedValidations[3].Message);
			Assert.AreEqual("CreatedOn", failedValidations[3].Name);
			Assert.AreEqual("The address sync ID is required.", failedValidations[4].Message);
			Assert.AreEqual("AddressSyncId", failedValidations[4].Name);

			// Run validate methods
			Assert.AreEqual(false, validator.TryValidate(address));
			TestHelper.ExpectedException<ValidationException>(() => validator.Validate(address),
				"The name must be provided.",
				"The name must be between a minimum of 1 and a maximum length of 5.",
				"The Id must be set.",
				"The CreatedOn value must be provided.",
				"The address sync ID is required."
			);

			address.Id = 1;
			address.Name = "A";
			address.CreatedOn = DateTime.MinValue.AddTicks(1);
			address.AddressSyncId = Guid.NewGuid();

			failedValidations = validator.Process(address).ToList();
			Assert.AreEqual(0, failedValidations.Count);

			// Run validate methods, validate should not throw exception
			Assert.AreEqual(true, validator.TryValidate(address));
			validator.Validate(address);
		}

		#endregion
	}
}