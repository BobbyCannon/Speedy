#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class ObjectExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void ToModel()
		{
			var defaultEntity = new Address();
			var nonDefaultEntity = new Address();
			nonDefaultEntity.UpdateWithNonDefaultValues();
			var properties = defaultEntity
				.GetCachedProperties()
				//.Where(x => x.Name != nameof(GrowattInverterEntity.History))
				.ToList();

			foreach (var property in properties)
			{
				var defaultValue = property.GetValue(defaultEntity);
				var nonDefaultValue = property.GetValue(nonDefaultEntity);
				TestHelper.AreNotEqual($"{property.Name}: {property.PropertyType.FullName}", defaultValue, nonDefaultValue);
			}
		}

		#endregion
	}
}