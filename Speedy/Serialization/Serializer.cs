#region References

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Serialization
{
	/// <summary>
	/// Represents a serializer
	/// </summary>
	public static class Serializer
	{
		#region Fields

		private static readonly SerializerSettings _settingsForDeepClone1, _settingsForDeepClone2;
		private static readonly SerializerSettings _settingsForDeserialization;

		#endregion

		#region Constructors

		static Serializer()
		{
			_settingsForDeserialization = new SerializerSettings(false, false, false, false, false, false);
			_settingsForDeepClone1 = new SerializerSettings(ignoreVirtuals: true);
			_settingsForDeepClone2 = new SerializerSettings(ignoreVirtuals: false);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Deep clone the item.
		/// </summary>
		/// <typeparam name="T"> The type to clone. </typeparam>
		/// <param name="item"> The item to clone. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore the virtual properties. </param>
		/// <returns> The clone of the item. </returns>
		public static T DeepClone<T>(this T item, bool ignoreVirtuals = false)
		{
			return FromJson<T>(item.ToJson(ignoreVirtuals ? _settingsForDeepClone1 : _settingsForDeepClone2));
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <typeparam name="T"> The type to convert into. </typeparam>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <returns> The deserialized object. </returns>
		public static T FromJson<T>(this string item)
		{
			return JsonConvert.DeserializeObject<T>(item, _settingsForDeserialization.JsonSettings);
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <param name="type"> The type to convert into. </param>
		/// <returns> The deserialized object. </returns>
		public static object FromJson(this string item, Type type)
		{
			return FromJson(item, type, _settingsForDeserialization.JsonSettings);
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <param name="type"> The type to convert into. </param>
		/// <param name="settings"> The settings to be used. </param>
		/// <returns> The deserialized object. </returns>
		public static object FromJson(this string item, Type type, SerializerSettings settings)
		{
			return FromJson(item, type, settings.JsonSettings);
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <param name="type"> The type to convert into. </param>
		/// <param name="settings"> The settings to be used. </param>
		/// <returns> The deserialized object. </returns>
		public static object FromJson(this string item, Type type, JsonSerializerSettings settings)
		{
			return string.IsNullOrWhiteSpace(item) ? null : JsonConvert.DeserializeObject(item, type, settings);
		}

		/// <summary>
		/// Determines if the string is a JSON string.
		/// </summary>
		/// <param name="input"> The value to validate. </param>
		/// <returns> True if the input is JSON or false if otherwise. </returns>
		public static bool IsJson(this string input)
		{
			input = input.Trim();

			var isWellFormed = new Func<bool>(() =>
			{
				try
				{
					JToken.Parse(input);
				}
				catch
				{
					return false;
				}

				return true;
			});

			return (input.StartsWith("{") && input.EndsWith("}")
					|| input.StartsWith("[") && input.EndsWith("]")
					|| input.StartsWith("\"") && input.EndsWith("\"")
				) && isWellFormed();
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="settings"> The settings for the serializer. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, SerializerSettings settings)
		{
			return JsonConvert.SerializeObject(item, settings.Indented ? Formatting.Indented : Formatting.None, settings.JsonSettings);
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> True to ignore members that are read only. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. Default value is false. </param>
		/// <param name="convertEnumsToString"> True to convert enumerations to strings value instead. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, bool indented = false, bool camelCase = false, bool ignoreNullValues = false, bool ignoreReadOnly = false, bool ignoreVirtuals = false, bool convertEnumsToString = false)
		{
			return item.ToJson(new SerializerSettings(indented, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString));
		}

		/// <summary>
		/// Unwraps a sync entity and disconnects it from the Entity Framework context. Check the value to see if the
		/// IUnwrappable interface is implemented. If so the value's implementation is used instead.
		/// The default behavior is to ignore read only and virtual properties.
		/// </summary>
		/// <typeparam name="T"> The type of the incoming object. </typeparam>
		/// <typeparam name="T2"> The type of the outgoing object. </typeparam>
		/// <param name="value"> The value to unwrap from the proxy. </param>
		/// <param name="update"> An optional update method. </param>
		/// <returns> The disconnected entity. </returns>
		public static T2 Unwrap<T, T2>(this T value, Action<T2> update = null)
		{
			// notice: do not use Unwrappable until it can support to specific types

			var response = value.ToJson(ignoreReadOnly: true, ignoreVirtuals: true).FromJson<T2>();
			update?.Invoke(response);
			return response;
		}

		/// <summary>
		/// Unwraps a sync entity and disconnects it from the Entity Framework context.
		/// </summary>
		/// <param name="value"> The value to unwrap from the proxy. </param>
		/// <param name="type"> The type of the outgoing object. </param>
		/// <returns> The disconnected entity. </returns>
		public static object Unwrap(this object value, Type type)
		{
			if (value is IUnwrappable unwrappable)
			{
				return unwrappable.Unwrap();
			}

			return value.ToJson(ignoreReadOnly: true, ignoreVirtuals: true).FromJson(type);
		}

		#endregion
	}
}