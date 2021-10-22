#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for enumerations
	/// </summary>
	public static class EnumExtensions
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, string> _enumErrorCache;

		#endregion

		#region Constructors

		static EnumExtensions()
		{
			_enumErrorCache = new ConcurrentDictionary<string, string>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clear the "flagged" enum value.
		/// </summary>
		/// <typeparam name="T"> The type of the enum value. </typeparam>
		/// <param name="value"> The value to update. </param>
		/// <param name="flag"> The flag to be cleared. </param>
		/// <returns> The value with the flagged cleared. </returns>
		public static T ClearFlag<T>(this T value, T flag) where T : Enum
		{
			return value.SetFlag(flag, false);
		}

		/// <summary>
		/// Gets the display name of the enum value.
		/// </summary>
		/// <param name="enumerationValue"> The enum value to get the name for. </param>
		/// <param name="arguments"> Any optional values for the description value. </param>
		/// <returns> The description of the enum value. </returns>
		public static string GetDescription<T>(this T enumerationValue, params object[] arguments) where T : struct
		{
			var type = enumerationValue.GetType();
			if (!type.IsEnum)
			{
				throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
			}

			var key = $"{type.FullName}+{enumerationValue}";
			var value = _enumErrorCache.GetOrAdd(key, x =>
			{
				// Tries to find a DescriptionAttribute for a potential friendly name for the enum
				var memberInfo = type.GetMember(enumerationValue.ToString());

				if (memberInfo.Length > 0)
				{
					var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

					if (attrs.Length > 0)
					{
						// Pull out the description value
						return ((DescriptionAttribute) attrs[0]).Description;
					}
				}

				// If we have no description attribute, just return the ToString of the enum
				return enumerationValue.ToString();
			});

			return arguments.Any() ? string.Format(value, arguments) : value;
		}

		/// <summary>
		/// Set the "flagged" enum value.
		/// </summary>
		/// <typeparam name="T"> The type of the enum value. </typeparam>
		/// <param name="value"> The value to update. </param>
		/// <param name="flag"> The flag to be set. </param>
		/// <returns> The value with the flagged set. </returns>
		public static T SetFlag<T>(this T value, T flag) where T : Enum
		{
			return value.SetFlag(flag, true);
		}

		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <param name="value"> The enum value to get the name for. </param>
		/// <returns> The name of the value. </returns>
		public static string ToDisplayName(this Enum value)
		{
			var type = value.GetType();
			var attribute = type
				.GetMember(value.ToString())
				.FirstOrDefault()?
				.GetCustomAttribute<DisplayAttribute>();

			return attribute == null ? Enum.GetName(type, value) : attribute.Name;
		}

		/// <summary>
		/// Gets the display names for an enum.
		/// </summary>
		/// <typeparam name="T"> The enum value to get the name for. </typeparam>
		/// <returns> The name of the value. </returns>
		public static Dictionary<string, string> ToDisplayNames<T>() where T : Enum
		{
			var type = typeof(T);
			var names = Enum.GetNames(type);
			var values = names
				.Select(x =>
				{
					var attribute = type
						.GetMember(x)
						.FirstOrDefault()?
						.GetCustomAttribute<DisplayAttribute>();

					return new { Key = attribute?.Name ?? x, Value = attribute?.ShortName ?? x };
				});

			return values.ToDictionary(x => x.Key, x => x.Value);
		}

		/// <summary>
		/// Gets the display short name.
		/// </summary>
		/// <param name="value"> The enum value to get the short name for. </param>
		/// <returns> The short name of the value. </returns>
		public static string ToDisplayShortName(this Enum value)
		{
			var type = value.GetType();
			var attribute = type
				.GetMember(value.ToString())
				.FirstOrDefault()?
				.GetCustomAttribute<DisplayAttribute>();

			return attribute == null ? Enum.GetName(type, value) : attribute.ShortName;
		}

		private static T SetFlag<T>(this T value, T flag, bool set) where T : Enum
		{
			var eValue = Convert.ToUInt64(value);
			var eFlag = Convert.ToUInt64(flag);
			var fValue = set ? eValue | eFlag : eValue & ~eFlag;
			return (T) Enum.ToObject(typeof(T), fValue);
		}

		#endregion
	}
}