#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

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

		Assert.AreEqual(0, nonDefaultEntity.Id);
		Assert.AreEqual(Guid.Empty, nonDefaultEntity.SyncId);

		var properties = Cache.GetSettablePropertiesPublicOnly(defaultEntity)
			.Where(x => x.Name is not nameof(Address.Id) and not nameof(Address.SyncId))
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