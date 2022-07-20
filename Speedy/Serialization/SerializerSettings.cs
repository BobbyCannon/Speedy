#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Speedy.Extensions;
using Speedy.Serialization.Converters;

#endregion

namespace Speedy.Serialization
{
	/// <summary>
	/// Represents serialization settings
	/// </summary>
	public class SerializerSettings : Bindable
	{
		private readonly Action<SerializerSettings> _beginReset, _endReset;

		#region Constants

		internal const bool DefaultForCamelCase = false;
		internal const bool DefaultForConvertEnumsToString = false;
		internal const bool DefaultForIgnoreNullValues = false;
		internal const bool DefaultForIgnoreReadOnly = false;
		internal const bool DefaultForIgnoreVirtuals = false;
		internal const bool DefaultForIndented = false;

		#endregion

		#region Fields

		private readonly Dictionary<Type, HashSet<string>> _cachedIgnoredProperties;
		private readonly HashSet<string> _globalIgnoredMembers;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a set of settings for the serializer.
		/// </summary>
		public SerializerSettings() : this(null)
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
		/// <param name="beginReset"> An optional beginning initializer for when resetting. </param>
		/// <param name="endReset"> An optional ending initializer for when resetting. </param>
		public SerializerSettings(bool indented = DefaultForIndented, bool camelCase = DefaultForCamelCase,
			bool ignoreNullValues = DefaultForIgnoreNullValues, bool ignoreReadOnly = DefaultForIgnoreReadOnly,
			bool ignoreVirtuals = DefaultForIgnoreVirtuals, bool convertEnumsToString = DefaultForConvertEnumsToString,
			Action<SerializerSettings> beginReset = null, Action<SerializerSettings> endReset = null)
			: this(new JsonSerializerSettings(), indented, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString, beginReset, endReset)
		{
		}

		private SerializerSettings(JsonSerializerSettings settings, bool indented = DefaultForIndented, bool camelCase = DefaultForCamelCase,
			bool ignoreNullValues = DefaultForIgnoreNullValues, bool ignoreReadOnly = DefaultForIgnoreReadOnly, bool ignoreVirtuals = DefaultForIgnoreVirtuals,
			bool convertEnumsToString = DefaultForConvertEnumsToString, Action<SerializerSettings> beginReset = null, Action<SerializerSettings> endReset = null)
		{
			Indented = indented;
			CamelCase = camelCase;
			IgnoreNullValues = ignoreNullValues;
			IgnoreReadOnly = ignoreReadOnly;
			IgnoreVirtuals = ignoreVirtuals;
			ConvertEnumsToString = convertEnumsToString;
			JsonSettings = settings ?? new JsonSerializerSettings();

			_cachedIgnoredProperties = new Dictionary<Type, HashSet<string>>();
			_globalIgnoredMembers = new HashSet<string>();
			_beginReset = beginReset;
			_beginReset?.Invoke(this);
			
			UpdateJsonSerializerSettings();

			_endReset = endReset;
			_endReset?.Invoke(this);
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
		[Obsolete("Use Serializer.DefaultSettings instead.")]
		public static SerializerSettings DefaultSettings => Serializer.DefaultSettings;

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
		/// Add or update converter to the JsonSettings.
		/// </summary>
		/// <param name="converter"> The converter to add or update. </param>
		public void AddOrUpdateConverter(JsonConverter converter)
		{
			if (JsonSettings == null)
			{
				return;
			}

			RemoveConverter(converter.GetType());
			JsonSettings.Converters.Add(converter);
		}

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

		/// <inheritdoc />
		public override void OnPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(CamelCase):
				case nameof(ConvertEnumsToString):
				case nameof(IgnoreNullValues):
				case nameof(IgnoreReadOnly):
				case nameof(IgnoreVirtuals):
				case nameof(Indented):
				{
					UpdateJsonSerializerSettings();
					break;
				}
			}

			base.OnPropertyChanged(propertyName);
		}

		/// <summary>
		/// Remove a converter of the provided type
		/// </summary>
		public void RemoveConverter<T>()
		{
			RemoveConverter(typeof(T));
		}

		/// <summary>
		/// Remove a converter of the provided type
		/// </summary>
		/// <param name="converterType"> </param>
		public void RemoveConverter(Type converterType)
		{
			if (JsonSettings == null)
			{
				return;
			}

			// Try to find the converter of provided type
			var existingConverter = JsonSettings.Converters.FirstOrDefault(x => x.GetType() == converterType);

			// Continue to remove until we have removed them all
			while (existingConverter != null)
			{
				JsonSettings.Converters.Remove(existingConverter);
				existingConverter = JsonSettings.Converters.FirstOrDefault(x => x.GetType() == converterType);
			}
		}

		/// <summary>
		/// Reset the state of shared settings
		/// </summary>
		public void Reset()
		{
			UpdateWith(Serializer.DefaultSettings);
			HasChanges = false;
		}
		
		/// <summary>
		/// Reset the state of the "DefaultSettings" back to initial state.
		/// </summary>
		internal void ResetForDefaultSettings()
		{
			// Reset
			_cachedIgnoredProperties.Clear();
			_globalIgnoredMembers.Clear();

			// Reset the settings
			CamelCase = DefaultForCamelCase;
			ConvertEnumsToString = DefaultForConvertEnumsToString;
			IgnoreNullValues = DefaultForIgnoreNullValues;
			IgnoreReadOnly = DefaultForIgnoreReadOnly;
			IgnoreVirtuals = DefaultForIgnoreVirtuals;
			Indented = DefaultForIndented;
			JsonSettings = new JsonSerializerSettings();

			UpdateJsonSerializerSettings();

			HasChanges = false;
		}
		
		/// <summary>
		/// Reset the DefaultSettings back to default settings.
		/// </summary>
		[Obsolete("Use Serializer.ResetDefaultSettings instead.")]
		public static void ResetDefaultSettings()
		{
			Serializer.ResetDefaultSettings();
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
				JsonSettings = new JsonSerializerSettings();
			}
			else
			{
				this.IfThen(_ => !exclusions.Contains(nameof(CamelCase)), x => x.CamelCase = update.CamelCase);
				this.IfThen(_ => !exclusions.Contains(nameof(ConvertEnumsToString)), x => x.ConvertEnumsToString = update.ConvertEnumsToString);
				this.IfThen(_ => !exclusions.Contains(nameof(IgnoreNullValues)), x => x.IgnoreNullValues = update.IgnoreNullValues);
				this.IfThen(_ => !exclusions.Contains(nameof(IgnoreReadOnly)), x => x.IgnoreReadOnly = update.IgnoreReadOnly);
				this.IfThen(_ => !exclusions.Contains(nameof(IgnoreVirtuals)), x => x.IgnoreVirtuals = update.IgnoreVirtuals);
				this.IfThen(_ => !exclusions.Contains(nameof(Indented)), x => x.Indented = update.Indented);
				this.IfThen(_ => !exclusions.Contains(nameof(JsonSettings)), x => x.JsonSettings = new JsonSerializerSettings());
			}

			UpdateJsonSerializerSettings();
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

		/// <summary>
		/// Configure the JsonSettings using our SerializerSettings values.
		/// </summary>
		/// <returns> The serialization settings. </returns>
		private void UpdateJsonSerializerSettings()
		{
			if (JsonSettings == null)
			{
				return;
			}

			_cachedIgnoredProperties.Clear();
			_globalIgnoredMembers.Clear();

			_beginReset?.Invoke(this);

			try
			{
				var namingStrategy = CamelCase
					? (NamingStrategy)new CamelCaseNamingStrategy()
					: new DefaultNamingStrategy();

				if (ConvertEnumsToString)
				{
					AddOrUpdateConverter(new StringEnumConverter { NamingStrategy = namingStrategy });
				}
				else
				{
					RemoveConverter(typeof(StringEnumConverter));
				}

				AddOrUpdateConverter(new IsoDateTimeConverter());
				AddOrUpdateConverter(new PartialUpdateConverter());

				JsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
				JsonSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
				JsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
				JsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
				JsonSettings.NullValueHandling = IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
				JsonSettings.ContractResolver = new JsonContractResolver(CamelCase, GetIgnoredProperties, IgnoreMember);
			}
			finally
			{
				_endReset?.Invoke(this);
			}
		}

		private HashSet<string> GetIgnoredProperties(Type type)
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

		private bool IgnoreMember(string member)
		{
			return _globalIgnoredMembers.Contains(member);
		}

		#endregion
	}
}