#region References

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Speedy.Configuration;
using Speedy.Exceptions;
using Speedy.Extensions;

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
				?? throw new SpeedyException($"Failed to find the '{nameof(Database.Property)}' method on the '{nameof(Database)}' class.");

			var databaseHasRequiredMethod = methods.FirstOrDefault(x => x.IsGenericMethod && x.Name == nameof(Database.HasRequired))
				?? throw new SpeedyException($"Failed to find the '{nameof(Database.HasRequired)}' method on the '{nameof(Database)}' class.");

			var assembly = database.GetMappingAssembly();
			var types = assembly.GetTypes();
			var mappingTypes = types.Where(x => !x.IsAbstract && x.GetInterfaces().Any(y => y == typeof(IEntityMappingConfiguration)));
			var builder = new ModelBuilder(new ConventionSet());

			foreach (var config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
			{
				var entityBuilder = (EntityTypeBuilder) config.Map(builder);
				var primaryKey = entityBuilder.Metadata.ClrType.GetPrimaryKeyType();

				if (primaryKey == null)
				{
					throw new SpeedyException($"Failed to find the 'Primary Key' type for the '{entityBuilder.Metadata.ClrType.FullName}' entity.");
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
					propertyConfiguration.MemberName = property.Name;

					foreach (var annotation in propertyAnnotations)
					{
						if (annotation.Name == "MaxLength")
						{
							propertyConfiguration.HasMaximumLength((int) annotation.Value);
						}
					}

					foreach (var mutableIndex in indexes.Where(x => x.IsUnique && x.Properties.Any(p => p.Name == property.Name)))
					{
						var index = mutableIndex as Index;
						if (index == null)
						{
							continue;
						}

						var name = index.Builder.Metadata.FindAnnotation("Relational:Name")?.Value.ToString() ?? index.ToString();
						var filter = index.Builder.Metadata.FindAnnotation("Relational:Filter")?.Value.ToString().ToLower() ?? string.Empty;
						var indexConfiguration = database.HasIndex(name);
						indexConfiguration.IsUnique();
						indexConfiguration.AllowNull = filter.Contains("is not null");
						indexConfiguration.AddProperty(propertyConfiguration);
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
					
					// Get the types for the property configurations
					var firstType = foreignKey.DeclaringEntityType.ClrType;
					var secondType = foreignKey.DeclaringEntityType.ClrType.GetPrimaryKeyType();
					var thirdType = foreignKey.PrincipalEntityType.ClrType;
					var fourthType = foreignKey.PrincipalEntityType.ClrType.GetPrimaryKeyType();
					
					// Get the configuration for the property
					var method = databaseHasRequiredMethod.MakeGenericMethod(firstType, secondType, thirdType, fourthType);
					var configuration = (IPropertyConfiguration) method.Invoke(database, new object[] { foreignKey.IsRequired, firstParameter, secondParameter, thirdParameter });

					switch (foreignKey.DeleteBehavior)
					{
						case DeleteBehavior.ClientSetNull:
						case DeleteBehavior.SetNull:
							configuration.OnDelete(RelationshipDeleteBehavior.SetNull);
							break;

						case DeleteBehavior.Restrict:
							configuration.OnDelete(RelationshipDeleteBehavior.Restrict);
							break;
						
						case DeleteBehavior.Cascade:
							configuration.OnDelete(RelationshipDeleteBehavior.Cascade);
							break;
					}
				}
			}
		}

		private static Type GetPrimaryKeyType(this Type type)
		{
			while (true)
			{
				var baseType = type.BaseType;
				if (baseType == null)
				{
					return null;
				}

				var arg = baseType.GenericTypeArguments.FirstOrDefault();
				if (arg == null)
				{
					type = baseType;
					continue;
				}

				return arg;
			}
		}
		
		private static Expression GetPropertyExpression(this Type type, string name)
		{
			var parameter = Expression.Parameter(type, "x");
			var memberExpression = Expression.Property(parameter, name);
			var lambdaExpression = Expression.Lambda(memberExpression, parameter);
			return lambdaExpression;
		}

		private static Expression GetPropertyObjectExpression(this Type type, string name)
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