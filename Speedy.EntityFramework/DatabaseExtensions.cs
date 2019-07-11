#region References

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Speedy.Configuration;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Extensions for the Database class.
	/// </summary>
	public static class DatabaseExtensions
	{
		#region Methods

		/// <summary>
		/// Try to configure the entity models using the EntityMappingConfiguration configurations
		/// </summary>
		/// <param name="database"> </param>
		public static void ConfigureModelViaMapping(this Database database)
		{
			var methods = database.GetCachedMethods(BindingFlags.Instance | BindingFlags.Public);
			var databasePropertyMethod = methods.FirstOrDefault(x => x.IsGenericMethod && x.Name == nameof(Database.Property) && x.ReturnType.Name == "PropertyConfiguration`2")
				?? throw new Exception($"Failed to find the '{nameof(Database.Property)}' method on the '{nameof(Database)}' class.");

			var databaseHasRequiredMethod = methods.FirstOrDefault(x => x.IsGenericMethod && x.Name == nameof(Database.HasRequired))
				?? throw new Exception($"Failed to find the '{nameof(Database.HasRequired)}' method on the '{nameof(Database)}' class.");

			var databaseHasOptionalMethod = methods.FirstOrDefault(x => x.IsGenericMethod && x.Name == nameof(Database.HasOptional))
				?? throw new Exception($"Failed to find the '{nameof(Database.HasOptional)}' method on the '{nameof(Database)}' class.");

			var assembly = database.GetMappingAssembly();
			var types = assembly.GetTypes();
			var mappingTypes = types.Where(x => !x.IsAbstract && x.GetInterfaces().Any(y => y == typeof(IEntityMappingConfiguration)));
			var builder = new ModelBuilder(new ConventionSet());

			foreach (var config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
			{
				var entityBuilder = (EntityTypeBuilder) config.Map(builder);
				var primaryKey = entityBuilder.Metadata.ClrType.BaseType?.GenericTypeArguments.FirstOrDefault();

				if (primaryKey == null)
				{
					throw new Exception($"Failed to find the 'Primary Key' type for the '{entityBuilder.Metadata.ClrType.FullName}' entity.");
				}

				var entityProperties = entityBuilder.Metadata.GetProperties();
				var annotations = entityBuilder.Metadata.GetAnnotations();
				var indexes = entityBuilder.Metadata.GetIndexes().ToList();
				var foreignKeys = entityBuilder.Metadata.GetForeignKeys().ToList();

				foreach (var property in entityProperties)
				{
					// Builds the 'Property' method
					// ex. database.Property<AddressEntity, long>(x => x.City).IsRequired().HasMaximumLength(256);
					var propertyAnnotations = property.GetAnnotations();
					var lambdaExpression = GetPropertyObjectExpression(entityBuilder.Metadata.ClrType, property.Name);
					var propertyMethod = databasePropertyMethod.MakeGenericMethod(entityBuilder.Metadata.ClrType, primaryKey);
					var propertyConfiguration = (IPropertyConfiguration) propertyMethod.Invoke(database, new object[] { lambdaExpression });

					propertyConfiguration.IsNullable = property.IsNullable;

					foreach (var annotation in propertyAnnotations)
					{
						if (annotation.Name == "MaxLength")
						{
							propertyConfiguration.HasMaximumLength((int) annotation.Value);
						}
					}

					if (indexes.Any(x => x.IsUnique && x.Properties.Any(p => p.Name == property.Name)))
					{
						propertyConfiguration.IsUnique();
					}
				}

				// Builds relationships
				// ex. database.HasOptional<AddressEntity, long, AddressEntity, long>(x => x.LinkedAddress, x => x.LinkedAddressId, x => x.LinkedAddresses);
				//     database.HasRequired<PetEntity, PetEntity.PetKey, PersonEntity, int>(x => x.Owner, x => x.OwnerId, x => x.Owners);
				foreach (var foreignKey in foreignKeys)
				{
					var firstParameter = GetPropertyExpression(foreignKey.DeclaringEntityType.ClrType, foreignKey.DependentToPrincipal.Name);
					var secondParameter = GetPropertyObjectExpression(foreignKey.DeclaringEntityType.ClrType, foreignKey.Properties[0].Name);
					var thirdParameter = foreignKey.PrincipalToDependent != null ? GetPropertyExpression(foreignKey.PrincipalEntityType.ClrType, foreignKey.PrincipalToDependent.Name) : null;

					var firstType = foreignKey.DeclaringEntityType.ClrType;
					var secondType = foreignKey.DeclaringEntityType.ClrType.BaseType?.GenericTypeArguments.FirstOrDefault();
					var thirdType = foreignKey.PrincipalEntityType.ClrType;
					var fourthType = foreignKey.PrincipalEntityType.ClrType.BaseType?.GenericTypeArguments.FirstOrDefault();

					if (foreignKey.IsRequired)
					{
						var method = databaseHasRequiredMethod.MakeGenericMethod(firstType, secondType, thirdType, fourthType);
						method.Invoke(database, new object[] { firstParameter, secondParameter, thirdParameter });
					}
					else
					{
						var method = databaseHasOptionalMethod.MakeGenericMethod(firstType, secondType, thirdType, fourthType);
						method.Invoke(database, new object[] { firstParameter, secondParameter, thirdParameter });
					}
				}
			}
		}

		private static Expression GetPropertyExpression(Type type, string name)
		{
			var parameter = Expression.Parameter(type, "x");
			var memberExpression = Expression.Property(parameter, name);
			var lambdaExpression = Expression.Lambda(memberExpression, parameter);
			return lambdaExpression;
		}

		private static Expression GetPropertyObjectExpression(Type type, string name)
		{
			var parameter = Expression.Parameter(type, "x");
			var memberExpression = Expression.Property(parameter, name);
			var convert = Expression.Convert(memberExpression, typeof(object));
			var lambdaExpression = Expression.Lambda(convert, parameter);
			return lambdaExpression;
		}

		#endregion
	}
}