#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Application;
using Speedy.Configuration.CommandLine;
using Speedy.Devices.Location;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class UpdatableExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void UpdateableShouldUpdateAll()
	{
		var updateableType = typeof(IUpdatable);
		var assemblies = new[]
		{
			typeof(Database).Assembly,
			typeof(EntityFrameworkDatabase).Assembly,
			typeof(DeviceId).Assembly
		};
		var exclusions = new[]
		{
			// Will get back to these later
			typeof(CommandLineParser),
		};
		var typeExclusions = new Dictionary<Type, string[]>
		{
			//{ typeof(Location), new[] { nameof(Location.HorizontalFlags), nameof(Location.VerticalFlags) } },
			{ typeof(Bindable), new[] { nameof(Bindable.HasChanges) } },
			{ typeof(SerializerSettings), new[] { nameof(SerializerSettings.JsonSettings) } },
			{ typeof(CommandLineArgument), new[]
			{
				nameof(CommandLineArgument.DefaultValue),
				nameof(CommandLineArgument.HasDefaultValue),
				nameof(CommandLineArgument.WasFound)
			} }
		};
		var types = assemblies
			.SelectMany(s => s.GetTypes())
			.Where(t => !exclusions.Contains(t))
			.Where(t => updateableType.IsAssignableFrom(t))
			.Where(x =>
			{
				if (x.IsAbstract || x.IsInterface || x.ContainsGenericParameters)
				{
					return false;
				}

				if (x.GetConstructors().All(t => t.GetParameters().Any()))
				{
					return false;
				}

				return x.GetCachedMethods()
					.Any(m => m.Name == nameof(IUpdatable.UpdateWith));
			})
			.ToArray();

		foreach (var type in types)
		{
			type.FullName.Dump();

			var typeExclusion = typeExclusions
				.Where(x => x.Key.IsAssignableFrom(type))
				.SelectMany(x => x.Value)
				.ToArray();

			ValidateUpdatableModel(GetModelWithNonDefaultValues(type, typeExclusion), typeExclusion);
		}
	}

	[TestMethod]
	public void UpdateShouldUpdateAllMembers()
	{
		var destination = new AccountEntity();
		var source = EntityFactory.GetAccount();

		source.Id = 99;
		source.SyncId = Guid.NewGuid();
		source.IsDeleted = true;
		source.Address.Id = 199;
		source.AddressId = 199;

		Assert.AreNotEqual(destination.Address, source.Address);
		Assert.AreNotEqual(destination.AddressId, source.AddressId);
		Assert.AreNotEqual(destination.AddressSyncId, source.AddressSyncId);
		Assert.AreNotEqual(destination.CreatedOn, source.CreatedOn);
		Assert.AreNotEqual(destination.Groups, source.Groups);
		Assert.AreNotEqual(destination.Id, source.Id);
		Assert.AreNotEqual(destination.IsDeleted, source.IsDeleted);
		Assert.AreNotEqual(destination.ModifiedOn, source.ModifiedOn);
		Assert.AreNotEqual(destination.Name, source.Name);
		Assert.AreNotEqual(destination.Pets, source.Pets);
		Assert.AreNotEqual(destination.SyncId, source.SyncId);

		// Update all members except virtual members
		destination.UpdateWithUsingReflection(source, typeof(AccountEntity).GetVirtualPropertyNames().ToArray());

		// All non virtual should be equal
		Assert.AreNotEqual(destination.Address, source.Address);
		Assert.AreEqual(destination.AddressId, source.AddressId);
		Assert.AreEqual(destination.AddressSyncId, source.AddressSyncId);
		Assert.AreEqual(destination.CreatedOn, source.CreatedOn);
		Assert.AreNotEqual(destination.Groups, source.Groups);
		Assert.AreEqual(destination.Id, source.Id);
		Assert.AreEqual(destination.IsDeleted, source.IsDeleted);
		Assert.AreEqual(destination.ModifiedOn, source.ModifiedOn);
		Assert.AreEqual(destination.Name, source.Name);
		Assert.AreNotEqual(destination.Pets, source.Pets);
		Assert.AreEqual(destination.SyncId, source.SyncId);

		// Update all members
		destination.UpdateWithUsingReflection(source);

		// All members should be equal now
		Assert.AreEqual(destination.Address, source.Address);
		Assert.AreEqual(destination.AddressId, source.AddressId);
		Assert.AreEqual(destination.AddressSyncId, source.AddressSyncId);
		Assert.AreEqual(destination.CreatedOn, source.CreatedOn);
		Assert.AreEqual(destination.Groups, source.Groups);
		Assert.AreEqual(destination.Id, source.Id);
		Assert.AreEqual(destination.IsDeleted, source.IsDeleted);
		Assert.AreEqual(destination.ModifiedOn, source.ModifiedOn);
		Assert.AreEqual(destination.Name, source.Name);
		Assert.AreEqual(destination.Pets, source.Pets);
		Assert.AreEqual(destination.SyncId, source.SyncId);
	}

	#endregion
}