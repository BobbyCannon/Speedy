#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using Castle.DynamicProxy.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.UnitTests
{
	public abstract class BaseModelTests<T> : BaseTests where T : new()
	{
		#region Methods

		protected T GetModelWithNonDefaultValues()
		{
			var response = new T();
			var random = RandomNumberGenerator.Create();
			var buffer = new byte[16];
			var properties = response.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToList();

			foreach (var property in properties)
			{
				var type = property.PropertyType;

				if (type.IsEnum)
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(response, BitConverter.ToInt32(buffer, 0));
				}
				else if (type == typeof(bool))
				{
					property.SetValue(response, true);
				}
				else if (type == typeof(DateTime))
				{
					property.SetValue(response, DateTime.UtcNow);
				}
				else if (type == typeof(byte))
				{
					random.GetBytes(buffer, 0, 1);
					property.SetValue(response, buffer[0]);
				}
				else if (type == typeof(short))
				{
					random.GetBytes(buffer, 0, 2);
					property.SetValue(response, BitConverter.ToInt16(buffer, 2));
				}
				else if (type == typeof(ushort))
				{
					random.GetBytes(buffer, 0, 2);
					property.SetValue(response, BitConverter.ToUInt16(buffer, 2));
				}
				else if (type == typeof(double))
				{
					random.GetBytes(buffer, 0, 8);
					property.SetValue(response, BitConverter.ToDouble(buffer, 0));
				}
				else if (type == typeof(Guid) || type == typeof(Guid?))
				{
					property.SetValue(response, Guid.NewGuid());
				}
				else if (type == typeof(int) || type == typeof(int?))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(response, BitConverter.ToInt32(buffer, 0));
				}
				else if (type == typeof(long) || type == typeof(long?))
				{
					random.GetBytes(buffer, 0, 8);
					property.SetValue(response, BitConverter.ToInt64(buffer, 0));
				}
				else if (type == typeof(string))
				{
					property.SetValue(response, Guid.NewGuid().ToString());
				}
				else if (type == typeof(TimeSpan))
				{
					property.SetValue(response, TimeSpan.FromSeconds(1));
				}
			}

			return response;
		}

		protected void ValidateModel(T model)
		{
			ValidateUnwrap(model);
			ValidateUpdateWith(model, false);
			// This ensure the exclusion validation is executed, no property will be name with "EmptyGuid" name
			ValidateUpdateWith(model, false, "00000000-0000-0000-0000-000000000000");
			ValidateUpdateWith(model, true);
			ValidateAllValuesAreNotDefault(model);
		}

		private object GetDefaultValue(Type propertyType)
		{
			var isCollection = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>);

			if (isCollection)
			{
				var collectionType = typeof(Collection<>);
				var constructedListType = collectionType.MakeGenericType(propertyType.GenericTypeArguments);
				return Activator.CreateInstance(constructedListType);
			}

			return propertyType.IsNullableType()
				|| propertyType == typeof(string)
					? null
					: Activator.CreateInstance(propertyType);
		}

		private void ValidateAllValuesAreNotDefault(T model)
		{
			var properties = model
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToList();

			foreach (var property in properties)
			{
				var value = property.GetValue(model);
				var defaultValue = GetDefaultValue(property.PropertyType);

				if (Equals(value, defaultValue))
				{
					throw new Exception($"Property {property.Name} should have been set but was not.");
				}
			}
		}

		private void ValidateUnwrap(T model)
		{
			if (model is IUnwrappable unwrappable)
			{
				var actual = unwrappable.Unwrap();
				TestHelper.AreEqual(model, actual);
			}

			if (model is Entity entity)
			{
				var actual = entity.Unwrap();
				TestHelper.AreEqual(model, actual);
			}
		}

		private void ValidateUpdateWith(T model, bool excludeVirtuals, params string[] exclusions)
		{
			if (model is not IUpdatable update)
			{
				return;
			}

			var actual = new T() as IUpdatable;
			var membersToIgnore = new List<string>();
			
			Assert.IsNotNull(actual);

			if (excludeVirtuals)
			{
				actual.UpdateWith(update, true, exclusions);
				membersToIgnore.AddRange(typeof(T).GetVirtualPropertyNames());
			}
			else
			{
				actual.UpdateWith(update, exclusions);
			}

			TestHelper.AreEqual(update, actual, membersToIgnore.ToArray());
		}

		#endregion
	}
}