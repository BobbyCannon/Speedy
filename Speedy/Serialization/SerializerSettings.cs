#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Speedy.Extensions;

#endregion

namespace Speedy.Serialization
{
	/// <summary>
	/// Represents serialization settings
	/// </summary>
	public class SerializerSettings : Bindable
	{
		#region Fields

		private readonly Dictionary<Type, HashSet<string>> _cachedIgnoredProperties;
		private readonly HashSet<string> _globalIgnoredMembers;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a set of settings for the serializer.
		/// </summary>
		public SerializerSettings() : this(false)
		{
		}

		/// <summary>
		/// Instantiates a set of settings for the serializer.
		/// </summary>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="ignoreNullValues"> The flag to determine to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> The flag to determine to ignore members that are readonly else include them. </param>
		/// <param name="ignoreVirtuals"> The flag to determine to ignore members that are virtual members else include them. </param>
		/// <param name="convertEnumsToString"> The flag to determine convert enums to string rather than number. </param>
		public SerializerSettings(bool indented = false, bool camelCase = false, bool ignoreNullValues = false, bool ignoreReadOnly = false, bool ignoreVirtuals = false, bool convertEnumsToString = false)
			: this(new JsonSerializerSettings(), indented, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString)
		{
			UpdateJsonSerializerSettings();
		}

		/// <summary>
		/// Instantiates a set of settings for the serializer.
		/// </summary>
		/// <param name="settings"> The initial settings. </param>
		public SerializerSettings(JsonSerializerSettings settings)
			: this(settings, false, false, false, false, false, false)
		{
			UpdateWithJsonSerializerSettings();
		}

		/// <summary>
		/// Defaults are set to the initial default state of the class.
		/// </summary>
		static SerializerSettings()
		{
			ResetDefaultSettings();
		}

		private SerializerSettings(JsonSerializerSettings settings, bool indented, bool camelCase, bool ignoreNullValues, bool ignoreReadOnly, bool ignoreVirtuals, bool convertEnumsToString)
		{
			Indented = indented;
			CamelCase = camelCase;
			IgnoreNullValues = ignoreNullValues;
			IgnoreReadOnly = ignoreReadOnly;
			IgnoreVirtuals = ignoreVirtuals;
			ConvertEnumsToString = convertEnumsToString;
			JsonSettings = settings;

			_cachedIgnoredProperties = new Dictionary<Type, HashSet<string>>();
			_globalIgnoredMembers = new HashSet<string>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The flag to determine if we should use camel case or not.
		/// </summary>
		public bool CamelCase { get; set; }

		/// <summary>
		/// The flag to determine convert enums to string rather than number.
		/// </summary>
		public bool ConvertEnumsToString { get; set; }

		/// <summary>
		/// Represents default values to set when <see cref="Reset" /> is invoked.
		/// </summary>
		public static SerializerSettings DefaultSettings { get; private set; }

		/// <summary>
		/// The flag to determine to ignore members that are null else include them.
		/// </summary>
		public bool IgnoreNullValues { get; set; }

		/// <summary>
		/// The flag to determine to ignore members that are readonly else include them.
		/// </summary>
		public bool IgnoreReadOnly { get; set; }

		/// <summary>
		/// The flag to determine to ignore members that are virtual members else include them.
		/// </summary>
		public bool IgnoreVirtuals { get; set; }

		/// <summary>
		/// The flag to determine if the JSON should be indented or not.
		/// </summary>
		public bool Indented { get; set; }

		/// <summary>
		/// The JSON setting for Newtonsoft.
		/// </summary>
		public JsonSerializerSettings JsonSettings { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Explicitly ignore the given property(s) for the given type
		/// </summary>
		/// <param name="type"> The type to ignore some properties on. </param>
		/// <param name="propertyNames"> One or more properties to ignore. Leave empty to ignore the type entirely. </param>
		public void Ignore(Type type, params string[] propertyNames)
		{
			if (!_cachedIgnoredProperties.ContainsKey(type))
			{
				_cachedIgnoredProperties[type] = new HashSet<string>();
			}

			var set = _cachedIgnoredProperties[type];

			foreach (var prop in propertyNames)
			{
				if (!set.Contains(prop))
				{
					set.Add(prop);
				}
			}

			HasChanges = true;
		}

		/// <summary>
		/// Explicitly ignore the given property(s) for all types
		/// </summary>
		/// <param name="propertyNames"> One or more properties to ignore. Leave empty to ignore the type entirely. </param>
		public void Ignore(params string[] propertyNames)
		{
			_globalIgnoredMembers.AddRange(propertyNames);

			HasChanges = true;
		}

		/// <summary>
		/// Reset the state of shared settings
		/// </summary>
		public void Reset()
		{
			// Reset
			_cachedIgnoredProperties.Clear();
			_globalIgnoredMembers.Clear();

			// Reset the JSON settings
			JsonSettings = new JsonSerializerSettings();

			// Update the new settings with defaults
			UpdateWith(DefaultSettings);

			HasChanges = false;
		}

		/// <summary>
		/// Reset the DefaultSettings back to default settings.
		/// </summary>
		public static void ResetDefaultSettings()
		{
			DefaultSettings = new SerializerSettings { HasChanges = false };
		}

		/// <summary>
		/// Update the SyncStatistics with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public void UpdateWith(SerializerSettings update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				CamelCase = update.CamelCase;
				ConvertEnumsToString = update.ConvertEnumsToString;
				IgnoreNullValues = update.IgnoreNullValues;
				IgnoreReadOnly = update.IgnoreReadOnly;
				IgnoreVirtuals = update.IgnoreVirtuals;
				Indented = update.Indented;
				JsonSettings = update.JsonSettings;
			}
			else
			{
				this.IfThen(x => !exclusions.Contains(nameof(CamelCase)), x => x.CamelCase = update.CamelCase);
				this.IfThen(x => !exclusions.Contains(nameof(ConvertEnumsToString)), x => x.ConvertEnumsToString = update.ConvertEnumsToString);
				this.IfThen(x => !exclusions.Contains(nameof(IgnoreNullValues)), x => x.IgnoreNullValues = update.IgnoreNullValues);
				this.IfThen(x => !exclusions.Contains(nameof(IgnoreReadOnly)), x => x.IgnoreReadOnly = update.IgnoreReadOnly);
				this.IfThen(x => !exclusions.Contains(nameof(IgnoreVirtuals)), x => x.IgnoreVirtuals = update.IgnoreVirtuals);
				this.IfThen(x => !exclusions.Contains(nameof(Indented)), x => x.Indented = update.Indented);
				this.IfThen(x => !exclusions.Contains(nameof(JsonSettings)), x => x.JsonSettings = update.JsonSettings);
			}
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case SerializerSettings options:
				{
					UpdateWith(options, exclusions);
					return;
				}
				default:
				{
					base.UpdateWith(update, exclusions);
					return;
				}
			}
		}

		private static void AddOrUpdateConverter(ICollection<JsonConverter> converters, JsonConverter converter)
		{
			RemoveConvertersOfTypes(converters, converter.GetType());
			converters.Add(converter);
		}

		private bool IgnoreMember(string member)
		{
			return _globalIgnoredMembers.Contains(member);
		}

		private HashSet<string> InitializeType(Type type)
		{
			if (_cachedIgnoredProperties.ContainsKey(type))
			{
				return _cachedIgnoredProperties[type];
			}

			var properties = new List<string>();

			if (IgnoreVirtuals)
			{
				properties.AddRange(type.GetVirtualPropertyNames());
			}

			if (IgnoreReadOnly)
			{
				var readOnly = type.GetCachedProperties().Where(x => x.CanRead && !x.CanWrite).Select(x => x.Name);
				properties.AddRange(readOnly);
			}

			Ignore(type, properties.ToArray());

			return _cachedIgnoredProperties[type];
		}

		private static void RemoveConvertersOfTypes(ICollection<JsonConverter> converters, Type jsonConverterType)
		{
			// Try to find the converter of provided type
			var existingStringEnumConverter = converters.FirstOrDefault(x => x.GetType() == jsonConverterType);

			// Continue to remove until we have removed them all
			while (existingStringEnumConverter != null)
			{
				converters.Remove(existingStringEnumConverter);
				existingStringEnumConverter = converters.FirstOrDefault(x => x.GetType() == jsonConverterType);
			}
		}

		/// <summary>
		/// Get the JSON serialization settings.
		/// </summary>
		/// <returns> The serialization settings. </returns>
		private void UpdateJsonSerializerSettings()
		{
			var namingStrategy = CamelCase ? (NamingStrategy) new CamelCaseNamingStrategy() : new DefaultNamingStrategy();

			if (ConvertEnumsToString)
			{
				AddOrUpdateConverter(JsonSettings.Converters, new StringEnumConverter { NamingStrategy = namingStrategy });
			}
			else
			{
				RemoveConvertersOfTypes(JsonSettings.Converters, typeof(StringEnumConverter));
			}

			AddOrUpdateConverter(JsonSettings.Converters, new IsoDateTimeConverter());

			JsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			JsonSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			JsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			JsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			JsonSettings.NullValueHandling = IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
			JsonSettings.ContractResolver = new JsonContractResolver(CamelCase, InitializeType, IgnoreMember);
		}

		/// <summary>
		/// Get the JSON serialization settings.
		/// </summary>
		private void UpdateWithJsonSerializerSettings()
		{
			// These values cannot be determined by JsonSettings: Indented, IgnoreReadOnly, IgnoreVirtuals
			var camelCaseNamingStrategyType = typeof(CamelCaseNamingStrategy);
			var stringEnumConverter = JsonSettings.GetConverter<StringEnumConverter>();
			var contractResolver = JsonSettings.ContractResolver as JsonContractResolver;

			CamelCase = (stringEnumConverter?.NamingStrategy?.GetType() == camelCaseNamingStrategyType)
				&& (contractResolver?.NamingStrategy?.GetType() == camelCaseNamingStrategyType);

			IgnoreNullValues = JsonSettings.NullValueHandling == NullValueHandling.Ignore;
			ConvertEnumsToString = stringEnumConverter != null;
		}

		#endregion
	}
}