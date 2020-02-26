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
	public class SerializerSettings : Bindable<SerializerSettings>
	{
		#region Fields

		private readonly Dictionary<Type, HashSet<string>> _cachedIgnoredProperties;
		private readonly HashSet<string> _globalIgnoredMembers;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a set of settings for the serializer.
		/// </summary>
		/// <param name="settings"> The initial settings. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="ignoreNullValues"> The flag to determine to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> The flag to determine to ignore members that are readonly else include them. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. Default value is false. </param>
		/// <param name="convertEnumsToString"> True to convert enumerations to strings value instead. </param>
		public SerializerSettings(JsonSerializerSettings settings, bool indented = false, bool camelCase = false, bool ignoreNullValues = false, bool ignoreReadOnly = false, bool ignoreVirtuals = false, bool convertEnumsToString = false)
		{
			JsonSettings = settings;
			Indented = indented;
			CamelCase = camelCase;
			IgnoreNullValues = ignoreNullValues;
			IgnoreReadOnly = ignoreReadOnly;
			IgnoreVirtuals = ignoreVirtuals;
			ConvertEnumsToString = convertEnumsToString;

			_cachedIgnoredProperties = new Dictionary<Type, HashSet<string>>();
			_globalIgnoredMembers = new HashSet<string>();

			UpdateJsonSerializerSettings(JsonSettings);
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
		}

		/// <summary>
		/// Defaults are set to the initial default state of the class.
		/// </summary>
		static SerializerSettings()
		{
			DefaultSettings = new SerializerSettings();
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
		public static SerializerSettings DefaultSettings { get; }

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
			if (!HasChanges)
			{
				return;
			}

			// Reset
			_cachedIgnoredProperties.Clear();
			_globalIgnoredMembers.Clear();

			// Set defaults
			UpdateWith(DefaultSettings);

			HasChanges = false;
		}

		/// <inheritdoc />
		public override void UpdateWith(SerializerSettings value, params string[] exclusions)
		{
			if (value == null)
			{
				return;
			}

			this.IfThen(x => !exclusions.Contains(nameof(CamelCase)), x => x.CamelCase = value.CamelCase);
			this.IfThen(x => !exclusions.Contains(nameof(ConvertEnumsToString)), x => x.ConvertEnumsToString = value.ConvertEnumsToString);
			this.IfThen(x => !exclusions.Contains(nameof(IgnoreNullValues)), x => x.IgnoreNullValues = value.IgnoreNullValues);
			this.IfThen(x => !exclusions.Contains(nameof(IgnoreReadOnly)), x => x.IgnoreReadOnly = value.IgnoreReadOnly);
			this.IfThen(x => !exclusions.Contains(nameof(IgnoreVirtuals)), x => x.IgnoreVirtuals = value.IgnoreVirtuals);
			this.IfThen(x => !exclusions.Contains(nameof(Indented)), x => x.Indented = value.Indented);

			JsonSettings = new JsonSerializerSettings();
			UpdateJsonSerializerSettings(JsonSettings);

			HasChanges = false;
		}

		/// <inheritdoc />
		public override void UpdateWith(object value, params string[] exclusions)
		{
			UpdateWith(value as SerializerSettings, exclusions);
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

		/// <summary>
		/// Get the JSON serialization settings.
		/// </summary>
		/// <param name="settings"> The settings for the JSON serializer. </param>
		/// <returns> The serialization settings. </returns>
		private void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
		{
			settings.Converters.Clear();

			var namingStrategy = CamelCase ? (NamingStrategy) new CamelCaseNamingStrategy() : new DefaultNamingStrategy();

			if (ConvertEnumsToString)
			{
				settings.Converters.Add(new StringEnumConverter { NamingStrategy = namingStrategy });
			}

			settings.Converters.Add(new IsoDateTimeConverter());
			settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			settings.NullValueHandling = IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
			settings.ContractResolver = new JsonContractResolver(CamelCase, InitializeType, IgnoreMember);
		}

		#endregion
	}
}