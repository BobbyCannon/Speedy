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
		public void GetDefault()
		{
			var scenarios = new Dictionary<Type, object>
			{
				{ typeof(byte), (byte) 0 },
				{ typeof(short), (short) 0 },
				{ typeof(ushort), (ushort) 0 },
				{ typeof(int), 0 },
				{ typeof(uint), (uint) 0 },
				{ typeof(long), (long) 0 },
				{ typeof(ulong), (ulong) 0 },
				{ typeof(string), null }
			};

			foreach (var scenario in scenarios)
			{
				Assert.AreEqual(scenario.Value, scenario.Key.GetDefaultValue());
			}
		}

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