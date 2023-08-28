#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class ObjectExtensionsTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ToModel()
		{
			var defaultEntity = new Address();
			var nonDefaultEntity = new Address();
			nonDefaultEntity.UpdateWithNonDefaultValues();
			// These should still be default
			Assert.AreEqual(0, nonDefaultEntity.Id);
			Assert.AreEqual(Guid.Empty, nonDefaultEntity.SyncId);
			// Must set Id, SyncId manually because they are excluded
			nonDefaultEntity.Id = 64;
			nonDefaultEntity.SyncId = Guid.NewGuid();
			var properties = defaultEntity
				.GetCachedProperties()
				.ToList();

			foreach (var property in properties)
			{
				var defaultValue = property.GetValue(defaultEntity);
				var nonDefaultValue = property.GetValue(nonDefaultEntity);
				AreNotEqual(defaultValue, nonDefaultValue, () => $"{property.Name}: {property.PropertyType.FullName}, {defaultValue}");
			}
		}

		#endregion
	}
}