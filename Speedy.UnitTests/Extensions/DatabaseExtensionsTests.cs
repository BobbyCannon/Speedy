#region References

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Data;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Website.Data;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class DatabaseExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void EnsureAllPropertiesAreMappedForClientDatabase()
		{
			var database = new ContosoClientMemoryDatabase();
			ValidateMappings(database);
		}

		[TestMethod]
		public void EnsureAllPropertiesAreMappedForEntityDatabase()
		{
			var database = new ContosoMemoryDatabase();
			ValidateMappings(database);
		}

		private static void ValidateMappings(Database database)
		{
			var assembly = database.GetMappingAssembly();
			var types = assembly.GetTypes();
			var mappingTypes = types.Where(x => !x.IsAbstract && x.GetInterfaces().Any(y => y == typeof(IEntityMappingConfiguration)));
			var builder = new ModelBuilder(new ConventionSet());
			var throwException = false;

			foreach (var config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
			{
				var entityBuilder = (EntityTypeBuilder) config.Map(builder);
				var mapProperties = entityBuilder.Metadata.GetProperties();
				var ignoreProperties = entityBuilder.Metadata.GetIgnoredMembers().ToList();
				var virtualProperties = entityBuilder.Metadata.ClrType.GetVirtualPropertyNames().ToList();
				var entityProperties = entityBuilder.Metadata.ClrType.GetCachedProperties().ToList();
				var missingProperties = entityProperties
					.Where(x => ignoreProperties.All(v => v != x.Name))
					.Where(x => virtualProperties.All(v => v != x.Name))
					.Where(x => x.CanWrite)
					.Where(x => mapProperties.All(m => m.Name != x.Name))
					.ToList();

				if (missingProperties.Count > 0)
				{
					throwException = true;
					entityBuilder.Metadata.Name.Dump();
					missingProperties.Select(x => "\t" + x.Name).Dump();
				}
			}

			if (throwException)
			{
				throw new Exception("An entity mapping is missing members.");
			}
		}

		#endregion
	}
}