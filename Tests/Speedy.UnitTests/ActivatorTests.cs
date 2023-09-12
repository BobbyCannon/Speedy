#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data.Location;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests;

[TestClass]
public class ActivatorTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	[SuppressMessage("ReSharper", "ConvertNullableToShortForm")]
	[SuppressMessage("ReSharper", "UseArrayEmptyMethod")]
	public void CreateInstance()
	{
		var scenarios = new List<(object expected, Type type)>
		{
			(new SpeedyList<int>(), typeof(SpeedyList<int>)),
			(new SpeedyList<int>(), typeof(ISpeedyList<int>)),
			(new List<int>(), typeof(ICollection<int>)),
			(new List<int>(), typeof(IEnumerable<int>)),
			(new List<int>(), typeof(IList<int>)),
			(new List<int>(), typeof(List<int>)),
			(new Collection<int>(), typeof(Collection<int>)),
			(new int[0], typeof(int[])),
			(new int?[0], typeof(int?[])),
			(new PartialUpdate<Account>(), typeof(PartialUpdate<Account>)),
			(new BasicLocation(), typeof(IBasicLocation)),
			(new HorizontalLocation(), typeof(IHorizontalLocation)),
			(new HorizontalLocation(), typeof(IMinimalHorizontalLocation)),
			(new VerticalLocation(), typeof(IVerticalLocation)),
			(new VerticalLocation(), typeof(IMinimalVerticalLocation)),
			(new Nullable<int>(), typeof(int?)),
			(new Nullable<EnumExtensions.EnumDetails>(), typeof(EnumExtensions.EnumDetails?)),
		};

		foreach (var scenario in scenarios)
		{
			var actual = Activator.CreateInstance(scenario.type);
			AreEqual(scenario.expected, actual);
		}
	}

	[TestMethod]
	public void CreateInstanceOfGeneric()
	{
		var scenarios = new List<(object expected, Type type, Type[] genericTypes)>
		{
			(new PartialUpdate<Account>(), typeof(PartialUpdate<>), new[] { typeof(Account) }),
			(new PartialUpdate<PartialUpdate<int>>(), typeof(PartialUpdate<>), new[] { typeof(PartialUpdate<int>) })
		};

		foreach (var scenario in scenarios)
		{
			var actual = Activator.CreateInstanceOfGeneric(scenario.type, scenario.genericTypes);
			AreEqual(scenario.expected, actual);
		}
	}

	[TestMethod]
	public void CreateInstanceUsingGenericType()
	{
		var expected = new PartialUpdate<Account>();
		var actual = Activator.CreateInstance<PartialUpdate<Account>>();
		AreEqual(expected, actual);
	}

	#endregion
}