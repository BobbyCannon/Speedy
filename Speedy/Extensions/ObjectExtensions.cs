#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the string type.
	/// </summary>
	public static class ObjectExtensions
	{
		#region Methods

		/// <summary>
		/// </summary>
		/// <param name="propertyType"> </param>
		/// <returns> </returns>
		public static object GetDefaultValue(this Type propertyType)
		{
			var isCollection = propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(ICollection<>));

			if (isCollection)
			{
				var collectionType = typeof(Collection<>);
				var constructedListType = collectionType.MakeGenericType(propertyType.GenericTypeArguments);
				return Activator.CreateInstance(constructedListType);
			}

			return propertyType.IsNullable()
				|| (propertyType == typeof(string))
					? null
					: Activator.CreateInstance(propertyType);
		}

		/// <summary>
		/// Executes a provided action if the test is successful.
		/// </summary>
		/// <param name="test"> The test to determine action to take. </param>
		/// <param name="action1"> The action to perform if the test is true. </param>
		/// <param name="action2"> The action to perform if the test is false. </param>
		public static void IfThenElse(Func<bool> test, Action action1, Action action2)
		{
			if (test())
			{
				action1();
			}
			else
			{
				action2();
			}
		}

		/// <summary>
		/// Update the provided object with non default values.
		/// </summary>
		/// <typeparam name="T"> The type of the value. </typeparam>
		/// <param name="value"> The value to update all properties for. </param>
		/// <param name="nonSupportedType"> An optional function to update non supported property value types. </param>
		/// <returns> </returns>
		public static T UpdateWithNonDefaultValues<T>(this T value, Func<PropertyInfo, object> nonSupportedType = null)
		{
			var random = RandomNumberGenerator.Create();
			var buffer = new byte[16];
			var properties = value
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToList();

			foreach (var property in properties)
			{
				var type = property.PropertyType;

				if (type.IsEnum)
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToInt32(buffer, 0));
				}
				else if (type == typeof(bool))
				{
					property.SetValue(value, true);
				}
				else if (type == typeof(DateTime))
				{
					property.SetValue(value, DateTime.UtcNow);
				}
				else if (type == typeof(byte))
				{
					random.GetBytes(buffer, 0, 1);
					property.SetValue(value, buffer[0]);
				}
				else if (type == typeof(short))
				{
					random.GetBytes(buffer, 0, 2);
					property.SetValue(value, BitConverter.ToInt16(buffer, 0));
				}
				else if (type == typeof(ushort))
				{
					random.GetBytes(buffer, 0, 2);
					property.SetValue(value, BitConverter.ToUInt16(buffer, 0));
				}
				else if (type == typeof(decimal))
				{
					var r = new Random();
					var dValue = NextDecimal(r);
					property.SetValue(value, dValue);
				}
				else if (type == typeof(double))
				{
					random.GetBytes(buffer, 0, 8);
					property.SetValue(value, BitConverter.ToDouble(buffer, 0));
				}
				else if (type == typeof(float))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToSingle(buffer, 0));
				}
				else if ((type == typeof(Guid)) || (type == typeof(Guid?)))
				{
					property.SetValue(value, Guid.NewGuid());
				}
				else if ((type == typeof(int)) || (type == typeof(int?)))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToInt32(buffer, 0));
				}
				else if ((type == typeof(uint)) || (type == typeof(uint?)))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToUInt32(buffer, 0));
				}
				else if ((type == typeof(long)) || (type == typeof(long?)))
				{
					random.GetBytes(buffer, 0, 8);
					property.SetValue(value, BitConverter.ToInt64(buffer, 0));
				}
				else if (type == typeof(string))
				{
					property.SetValue(value, Guid.NewGuid().ToString());
				}
				else if (type == typeof(TimeSpan))
				{
					property.SetValue(value, TimeSpan.FromSeconds(1));
				}
				else
				{
					if (nonSupportedType != null)
					{
						property.SetValue(value, nonSupportedType.Invoke(property));
					}
				}
			}

			return value;
		}

		/// <summary>
		/// Validates that all values are not default value.
		/// </summary>
		/// <typeparam name="T"> The type of the model. </typeparam>
		/// <param name="model"> The model to be validated. </param>
		/// <param name="exclusions"> An optional set of exclusions. </param>
		public static void ValidateAllValuesAreNotDefault<T>(this T model, params string[] exclusions)
		{
			var properties = model
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToList();

			foreach (var property in properties)
			{
				if ((exclusions.Length > 0) && exclusions.Contains(property.Name))
				{
					continue;
				}

				var value = property.GetValue(model);
				var defaultValue = GetDefaultValue(property.PropertyType);

				if (Equals(value, defaultValue))
				{
					throw new Exception($"Property {property.Name} should have been set but was not.");
				}
			}
		}

		private static decimal NextDecimal(this Random rng)
		{
			var scale = (byte) rng.Next(29);
			var sign = rng.Next(2) == 1;
			return new decimal(
				rng.NextInt32(),
				rng.NextInt32(),
				rng.NextInt32(),
				sign,
				scale);
		}

		/// <summary>
		/// Returns an Int32 with a random value across the entire range of
		/// possible values.
		/// </summary>
		private static int NextInt32(this Random rng)
		{
			var firstBits = rng.Next(0, 1 << 4) << 28;
			var lastBits = rng.Next(0, 1 << 28);
			return firstBits | lastBits;
		}

		#endregion
	}
}