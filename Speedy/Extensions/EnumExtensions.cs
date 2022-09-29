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

		private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<Enum, EnumDetails>> _cache;

		#endregion

		#region Constructors

		static EnumExtensions()
		{
			_cache = new ConcurrentDictionary<Type, IReadOnlyDictionary<Enum, EnumDetails>>();
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
		/// Returns the number of values for the provided enum.
		/// </summary>
		/// <returns> The number of values in the enum. </returns>
		public static int Count<T>() where T : Enum
		{
			return GetAllEnumDetails(typeof(T)).Count;
		}

		/// <summary>
		/// Returns the number of values for the provided enum.
		/// </summary>
		/// <param name="value"> The enum value to count. </param>
		/// <returns> The number of values in the enum. </returns>
		public static int Count(this Enum value)
		{
			return GetAllEnumDetails(value.GetType()).Count;
		}

		/// <summary>
		/// Gets the all details for an enum value.
		/// </summary>
		/// <returns> The all details for the enum value. </returns>
		public static IReadOnlyDictionary<T, EnumDetails> GetAllEnumDetails<T>() where T : Enum
		{
			return GetAllEnumDetails(typeof(T)).ToDictionary(x => (T) x.Key, x => x.Value);
		}

		/// <summary>
		/// Gets the all details for an enum value.
		/// </summary>
		/// <param name="type"> The type to process. </param>
		/// <returns> The all details for the enum value. </returns>
		public static IReadOnlyDictionary<Enum, EnumDetails> GetAllEnumDetails(Type type)
		{
			return _cache.GetOrAdd(type, x =>
			{
				var enumValues = Enum.GetValues(type);
				var response = new Dictionary<Enum, EnumDetails>();

				foreach (Enum enumValue in enumValues)
				{
					var memberInfo = x.GetMember(enumValue.ToString()).FirstOrDefault();
					var descriptionAttribute = memberInfo?.GetCustomAttribute<DescriptionAttribute>();
					var displayAttribute = memberInfo?.GetCustomAttribute<DisplayAttribute>();
					var details = new EnumDetails
					{
						Description = displayAttribute?.Description
							?? descriptionAttribute?.Description
							?? enumValue.ToString(),
						Name = displayAttribute?.Name ?? enumValue.ToString(),
						ShortName = displayAttribute?.ShortName ?? enumValue.ToString(),
						Value = enumValue
					};
					response.Add(enumValue, details);
				}

				return response;
			});
		}

		/// <summary>
		/// Gets the all details for an enum value except the excluded.
		/// </summary>
		/// <param name="exclusions"> The types to be excluded. </param>
		/// <returns> The all details for the enum value except the exclusions. </returns>
		public static IReadOnlyDictionary<T, EnumDetails> GetAllEnumDetailsExcept<T>(params T[] exclusions) where T : Enum
		{
			return GetAllEnumDetails(typeof(T))
				.Where(x => !exclusions.Contains((T) x.Key))
				.ToDictionary(x => (T) x.Key, x => x.Value);
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <param name="value"> The enum value to get the description for. </param>
		/// <returns> The description of the value. </returns>
		public static string GetDescription(this Enum value)
		{
			return GetEnumDetails(value).Description;
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <param name="value"> The enum value to get the description for. </param>
		/// <returns> The description of the value. </returns>
		public static string GetDescription<T>(this T value) where T : Enum
		{
			return GetEnumDetails(value).Description;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <param name="value"> The enum value to get the name for. </param>
		/// <returns> The name of the value. </returns>
		public static string GetDisplayName(this Enum value)
		{
			return GetEnumDetails(value).Name;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <param name="value"> The enum value to get the name for. </param>
		/// <returns> The name of the value. </returns>
		public static string GetDisplayName<T>(this T value) where T : Enum
		{
			return GetEnumDetails(value).Name;
		}

		/// <summary>
		/// Gets the display names.
		/// </summary>
		/// <returns> The display names of the enum type. </returns>
		public static string[] GetDisplayNames<T>() where T : Enum
		{
			return GetAllEnumDetails<T>().Select(x => x.Value.Name).ToArray();
		}

		/// <summary>
		/// Gets the display names excluding the provided values.
		/// </summary>
		/// <param name="exclusions"> An optional set of enums to exclude. </param>
		/// <returns> The display names of the enum type. </returns>
		public static string[] GetDisplayNamesExcept<T>(params T[] exclusions) where T : Enum
		{
			return GetAllEnumDetails<T>()
				.Where(x => !exclusions.Contains(x.Key))
				.Select(x => x.Value.Name)
				.ToArray();
		}

		/// <summary>
		/// Gets the display short name.
		/// </summary>
		/// <param name="value"> The enum value to get the short name for. </param>
		/// <returns> The name of the value. </returns>
		public static string GetDisplayShortName(this Enum value)
		{
			return GetEnumDetails(value).ShortName;
		}

		/// <summary>
		/// Gets the short name.
		/// </summary>
		/// <param name="value"> The enum value to get the short name for. </param>
		/// <returns> The short name of the value. </returns>
		public static string GetDisplayShortName<T>(this T value) where T : Enum
		{
			return GetEnumDetails(value).ShortName;
		}

		/// <summary>
		/// Gets the details for an enum value.
		/// </summary>
		/// <param name="value"> The value to process. </param>
		/// <returns> The details for the enum value. </returns>
		public static EnumDetails GetEnumDetails(this Enum value)
		{
			var allDetails = GetAllEnumDetails(value.GetType());
			return allDetails.ContainsKey(value)
				? allDetails[value]
				: new EnumDetails
				{
					Description = value.ToString(),
					Name = value.ToString(),
					ShortName = value.ToString(),
					Value = value
				};
		}

		/// <summary>
		/// Gets the type array of the values flagged (set) in the enum.
		/// </summary>
		/// <typeparam name="T"> The enum type. </typeparam>
		/// <param name="value"> The enum value to get the flagged values for. </param>
		/// <returns> The individual values for the enum. </returns>
		public static T[] GetFlaggedValues<T>(this T value) where T : Enum
		{
			var values = GetValues<T>();
			return values.Where(x => value?.HasFlag(x) == true).ToArray();
		}

		/// <summary>
		/// Gets the type array of the values in the enum.
		/// </summary>
		/// <typeparam name="T"> The enum type. </typeparam>
		/// <returns> The individual values for the enum. </returns>
		public static T[] GetFlagValues<T>() where T : Enum
		{
			return Enum.GetValues(typeof(T))
				.Cast<T>()
				.Where(v =>
				{
					// because enums can be UInt64
					var x = Convert.ToUInt64(v);
					return (x != 0) && ((x & (x - 1)) == 0);
					// Checks whether x is a power of 2
					// Example: when x = 16, the binary values are:
					// x:         10000
					// x-1:       01111
					// x & (x-1): 00000
				})
				.ToArray();
		}

		/// <summary>
		/// Gets the type array of the values in the enum.
		/// </summary>
		/// <typeparam name="T"> The enum type. </typeparam>
		/// <returns> The individual values for the enum. </returns>
		public static T[] GetValues<T>() where T : Enum
		{
			return typeof(T).GetEnumValues().Cast<T>().ToArray();
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
		/// Set the "flagged" enum value based on the provided update value state.
		/// </summary>
		/// <typeparam name="T"> The type of the enum value. </typeparam>
		/// <param name="value"> The value to update. </param>
		/// <param name="update"> The source of update. </param>
		/// <param name="flags"> The flags to be read then set. </param>
		/// <returns> The value with the flagged set or cleared based on the update value. </returns>
		public static T UpdateFlag<T>(this T value, T update, T flags) where T : Enum
		{
			var flagValues = flags.GetFlaggedValues();
			var response = value;

			foreach (var flag in flagValues)
			{
				response = update.HasFlag(flag)
					? response.SetFlag(flag)
					: response.ClearFlag(flag);
			}

			return response;
		}

		private static T SetFlag<T>(this T value, T flag, bool set) where T : Enum
		{
			var eValue = Convert.ToUInt64(value);
			var eFlag = Convert.ToUInt64(flag);
			var fValue = set ? eValue | eFlag : eValue & ~eFlag;
			return (T) Enum.ToObject(typeof(T), fValue);
		}

		#endregion

		#region Structures

		/// <summary>
		/// Represents the details for an enum value.
		/// </summary>
		public struct EnumDetails
		{
			#region Properties

			/// <summary>
			/// The description of the enum value.
			/// </summary>
			/// <remarks>
			/// Priority is [DisplayAttribute].Description, [DescriptionAttribute].Description, enum.ToString()
			/// </remarks>
			public string Description { get; set; }

			/// <summary>
			/// The name of the enum value.
			/// </summary>
			/// <remarks>
			/// Priority is [DisplayAttribute].Name, enum.ToString()
			/// </remarks>
			public string Name { get; set; }

			/// <summary>
			/// The short name of the enum value.
			/// </summary>
			/// <remarks>
			/// Priority is [DisplayAttribute].Short, enum.ToString()
			/// </remarks>
			public string ShortName { get; set; }

			/// <summary>
			/// The enum value.
			/// </summary>
			public Enum Value { get; set; }

			#endregion
		}

		#endregion
	}
}