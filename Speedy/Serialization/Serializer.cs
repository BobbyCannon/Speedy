#region References

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Serialization;

/// <summary>
/// Represents a serializer
/// </summary>
public static class Serializer
{
	#region Fields

	private static readonly SerializerSettings _settingsForDeepClone1, _settingsForDeepClone2;
	private static readonly SerializerSettings _settingsForDeserialization, _settingsForSerialization;
	private static readonly SerializerSettings _settingsForRawSerialization;

	#endregion

	#region Constructors

	static Serializer()
	{
		_settingsForRawSerialization = new SerializerSettings(endReset: x => x.JsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None);
		_settingsForDeserialization = new SerializerSettings();
		_settingsForSerialization = new SerializerSettings();
		_settingsForDeepClone1 = new SerializerSettings(beginReset: x => x.IgnoreVirtuals = true);
		_settingsForDeepClone2 = new SerializerSettings(beginReset: x => x.IgnoreVirtuals = false);

		DefaultSettings = new SerializerSettings();
		DefaultSettings.PropertyChanged += DefaultSettingsOnPropertyChanged;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Represents default values to set when <see cref="SerializerSettings.Reset" /> is invoked.
	/// </summary>
	public static SerializerSettings DefaultSettings { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Add or update converter to the JsonSettings.
	/// </summary>
	/// <param name="converter"> The converter to add or update. </param>
	public static void AddOrUpdateConverter(JsonConverter converter)
	{
		DefaultSettings.RemoveConverter(converter.GetType());
		DefaultSettings.JsonSettings.Converters.Add(converter);
	}

	/// <summary>
	/// Deep clone the item.
	/// </summary>
	/// <typeparam name="T"> The type to clone. </typeparam>
	/// <param name="item"> The item to clone. </param>
	/// <param name="maxDepth"> The max depth to clone. Defaults to null. </param>
	/// <param name="ignoreVirtuals"> Flag to ignore the virtual properties. </param>
	/// <returns> The clone of the item. </returns>
	public static T DeepClone<T>(this T item, int? maxDepth = null, bool ignoreVirtuals = false)
	{
		if (maxDepth != null)
		{
			var settings = new SerializerSettings(ignoreVirtuals: ignoreVirtuals) { JsonSettings = { MaxDepth = maxDepth } };
			return FromJson<T>(item.ToJson(settings));
		}

		return FromJson<T>(item.ToJson(ignoreVirtuals ? _settingsForDeepClone1 : _settingsForDeepClone2));
	}

	/// <summary>
	/// Deep clone the item.
	/// </summary>
	/// <param name="item"> The item to clone. </param>
	/// <param name="maxDepth"> The max depth to clone. Defaults to null. </param>
	/// <param name="ignoreVirtuals"> Flag to ignore the virtual properties. </param>
	/// <returns> The clone of the item. </returns>
	public static object DeepCloneObject(this object item, int? maxDepth = null, bool ignoreVirtuals = false)
	{
		if (maxDepth != null)
		{
			var settings = new SerializerSettings(ignoreVirtuals: ignoreVirtuals) { JsonSettings = { MaxDepth = maxDepth } };
			return FromJson(item.ToJson(settings), item.GetRealType());
		}

		return FromJson(item.ToJson(ignoreVirtuals ? _settingsForDeepClone1 : _settingsForDeepClone2), item.GetRealType());
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
	/// <typeparam name="T"> The type to convert into. </typeparam>
	/// <param name="item"> The JSON data to deserialize. </param>
	/// <param name="settings"> The settings to be used. </param>
	/// <returns> The deserialized object. </returns>
	public static T FromJson<T>(this string item, SerializerSettings settings)
	{
		return JsonConvert.DeserializeObject<T>(item, settings.JsonSettings);
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

		return ((input.StartsWith("{") && input.EndsWith("}"))
				|| (input.StartsWith("[") && input.EndsWith("]"))
				|| (input.StartsWith("\"") && input.EndsWith("\""))
			) && isWellFormed();
	}

	/// <summary>
	/// Determines if the string is a query string
	/// </summary>
	/// <param name="input"> The value to validate. </param>
	/// <returns> True if the input is a query string or false if otherwise. </returns>
	public static bool IsQueryString(this string input)
	{
		return (input != null)
			&& (input.Length >= 1)
			&& (input[0] == '?');
	}

	/// <summary>
	/// Reset the DefaultSettings back to default settings.
	/// </summary>
	public static void ResetDefaultSettings()
	{
		// Must use special reset to actual reset back to initial state
		DefaultSettings.ResetForDefaultSettings();

		// Now reset all dependents
		_settingsForDeepClone1.Reset();
		_settingsForDeepClone2.Reset();
		_settingsForDeserialization.Reset();
		_settingsForSerialization.Reset();
		_settingsForRawSerialization.Reset();
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
	/// <returns> The JSON string of the serialized object. </returns>
	public static string ToJson<T>(this T item)
	{
		return item.ToJson(_settingsForSerialization);
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
	public static string ToJson<T>(this T item, bool indented = SerializerSettings.DefaultForIndented, bool camelCase = SerializerSettings.DefaultForCamelCase,
		bool ignoreNullValues = SerializerSettings.DefaultForIgnoreNullValues, bool ignoreReadOnly = SerializerSettings.DefaultForIgnoreReadOnly,
		bool ignoreVirtuals = SerializerSettings.DefaultForIgnoreVirtuals, bool convertEnumsToString = SerializerSettings.DefaultForConvertEnumsToString)
	{
		return item.ToJson(new SerializerSettings(indented, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString));
	}

	/// <summary>
	/// Serialize an object into a JSON string.
	/// </summary>
	/// <typeparam name="T"> The type of the object to serialize. </typeparam>
	/// <param name="item"> The object to serialize. </param>
	/// <returns> The JSON string of the serialized object. </returns>
	public static string ToRawJson<T>(this T item)
	{
		return item.ToJson(_settingsForRawSerialization);
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
	public static string ToRawJson<T>(this T item, bool indented = SerializerSettings.DefaultForIndented, bool camelCase = SerializerSettings.DefaultForCamelCase,
		bool ignoreNullValues = SerializerSettings.DefaultForIgnoreNullValues, bool ignoreReadOnly = SerializerSettings.DefaultForIgnoreReadOnly,
		bool ignoreVirtuals = SerializerSettings.DefaultForIgnoreVirtuals, bool convertEnumsToString = SerializerSettings.DefaultForConvertEnumsToString)
	{
		return item.ToJson(new SerializerSettings(indented, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString) { JsonSettings = { PreserveReferencesHandling = PreserveReferencesHandling.None } });
	}

	/// <summary>
	/// Try to convert the string into an object.
	/// </summary>
	/// <param name="item"> The JSON data to deserialize. </param>
	/// <param name="type"> The type to convert into. </param>
	/// <param name="value"> The deserialized object. </param>
	/// <returns> True if the object was deserialized otherwise false. </returns>
	public static bool TryFromJson(this string item, Type type, out object value)
	{
		try
		{
			value = JsonConvert.DeserializeObject(item, type, _settingsForDeserialization.JsonSettings);
			return true;
		}
		catch
		{
			value = default;
			return false;
		}
	}

	/// <summary>
	/// Try to convert the string into an object.
	/// </summary>
	/// <typeparam name="T"> The type to convert into. </typeparam>
	/// <param name="item"> The JSON data to deserialize. </param>
	/// <param name="value"> The deserialized object. </param>
	/// <returns> True if the object was deserialized otherwise false. </returns>
	public static bool TryFromJson<T>(this string item, out T value)
	{
		try
		{
			value = JsonConvert.DeserializeObject<T>(item, _settingsForDeserialization.JsonSettings);
			return true;
		}
		catch
		{
			value = default;
			return false;
		}
	}

	private static void DefaultSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(SerializerSettings.CamelCase):
			{
				_settingsForDeserialization.CamelCase = DefaultSettings.CamelCase;
				_settingsForSerialization.CamelCase = DefaultSettings.CamelCase;
				_settingsForRawSerialization.CamelCase = DefaultSettings.CamelCase;
				_settingsForDeepClone1.CamelCase = DefaultSettings.CamelCase;
				_settingsForDeepClone2.CamelCase = DefaultSettings.CamelCase;
				break;
			}
			case nameof(SerializerSettings.ConvertEnumsToString):
			{
				_settingsForDeserialization.ConvertEnumsToString = DefaultSettings.ConvertEnumsToString;
				_settingsForSerialization.ConvertEnumsToString = DefaultSettings.ConvertEnumsToString;
				_settingsForRawSerialization.ConvertEnumsToString = DefaultSettings.ConvertEnumsToString;
				_settingsForDeepClone1.ConvertEnumsToString = DefaultSettings.ConvertEnumsToString;
				_settingsForDeepClone2.ConvertEnumsToString = DefaultSettings.ConvertEnumsToString;
				break;
			}
			case nameof(SerializerSettings.IgnoreNullValues):
			{
				_settingsForDeserialization.IgnoreNullValues = DefaultSettings.IgnoreNullValues;
				_settingsForSerialization.IgnoreNullValues = DefaultSettings.IgnoreNullValues;
				_settingsForRawSerialization.IgnoreNullValues = DefaultSettings.IgnoreNullValues;
				_settingsForDeepClone1.IgnoreNullValues = DefaultSettings.IgnoreNullValues;
				_settingsForDeepClone2.IgnoreNullValues = DefaultSettings.IgnoreNullValues;
				break;
			}
			case nameof(SerializerSettings.IgnoreReadOnly):
			{
				_settingsForDeserialization.IgnoreReadOnly = DefaultSettings.IgnoreReadOnly;
				_settingsForSerialization.IgnoreReadOnly = DefaultSettings.IgnoreReadOnly;
				_settingsForRawSerialization.IgnoreReadOnly = DefaultSettings.IgnoreReadOnly;
				_settingsForDeepClone1.IgnoreReadOnly = DefaultSettings.IgnoreReadOnly;
				_settingsForDeepClone2.IgnoreReadOnly = DefaultSettings.IgnoreReadOnly;
				break;
			}
			case nameof(SerializerSettings.IgnoreVirtuals):
			{
				_settingsForDeserialization.IgnoreVirtuals = DefaultSettings.IgnoreVirtuals;
				_settingsForSerialization.IgnoreVirtuals = DefaultSettings.IgnoreVirtuals;
				_settingsForRawSerialization.IgnoreVirtuals = DefaultSettings.IgnoreVirtuals;
				// These should never change for these settings
				//_settingsForDeepClone1.IgnoreVirtuals = DefaultSettings.IgnoreVirtuals;
				//_settingsForDeepClone2.IgnoreVirtuals = DefaultSettings.IgnoreVirtuals;
				break;
			}
			case nameof(SerializerSettings.Indented):
			{
				_settingsForDeserialization.Indented = DefaultSettings.Indented;
				_settingsForSerialization.Indented = DefaultSettings.Indented;
				_settingsForRawSerialization.Indented = DefaultSettings.Indented;
				_settingsForDeepClone1.Indented = DefaultSettings.Indented;
				_settingsForDeepClone2.Indented = DefaultSettings.Indented;
				break;
			}
		}
	}

	#endregion
}