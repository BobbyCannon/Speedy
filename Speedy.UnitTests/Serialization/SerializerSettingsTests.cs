#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Serialization.Converters;

#endregion

namespace Speedy.UnitTests.Serialization
{
	[TestClass]
	public class SerializerSettingsTests
	{
		#region Methods

		[TestMethod]
		public void CustomConvertersShouldBeRemovedDuringReset()
		{
			var jsonSettings = new JsonSerializerSettings();
			jsonSettings.Converters.Add(new VersionStringConverter());

			var settings = new SerializerSettings(jsonSettings);
			var actual = settings.JsonSettings.GetConverter<VersionStringConverter>();
			Assert.IsNotNull(actual, "The JSON converter was not found but should have been. ");

			// Reset should remove the custom converter because it's not in DefaultSettings
			settings.Reset();
			actual = settings.JsonSettings.GetConverter<VersionStringConverter>();
			Assert.IsNull(actual, "The JSON converter was found but should not have been. ");
		}

		[TestMethod]
		public void CustomConvertersShouldNotBeRemoved()
		{
			var jsonSettings = new JsonSerializerSettings();
			jsonSettings.Converters.Add(new VersionStringConverter());

			var settings = new SerializerSettings(jsonSettings);
			var actual = settings.JsonSettings.GetConverter<VersionStringConverter>();
			Assert.IsNotNull(actual, "The JSON converter was not found but should have been. ");
		}

		[TestMethod]
		public void JsonSerializerSettingsShouldSetDefaults()
		{
			var jsonSettings = new SerializerSettings(true, true, true, true, true, true).JsonSettings;
			var settings = new SerializerSettings(jsonSettings);
			Assert.AreEqual(false, settings.Indented);
			Assert.AreEqual(true, settings.CamelCase);
			Assert.AreEqual(true, settings.IgnoreNullValues);
			Assert.AreEqual(false, settings.IgnoreReadOnly);
			Assert.AreEqual(false, settings.IgnoreVirtuals);
			Assert.AreEqual(true, settings.ConvertEnumsToString);
		}

		[TestMethod]
		public void SettingsCombinations()
		{
			ValidateSettings(true, false, false, false, false, false);
			ValidateSettings(false, true, false, false, false, false);
			ValidateSettings(false, false, true, false, false, false);
			ValidateSettings(false, false, false, true, false, false);
			ValidateSettings(false, false, false, false, true, false);
			ValidateSettings(false, false, false, false, false, true);
			ValidateSettings(true, true, true, true, true, true);
		}

		[TestMethod]
		public void SettingsDefaults()
		{
			var settings = new SerializerSettings();
			Assert.AreEqual(false, settings.Indented);
			Assert.AreEqual(false, settings.CamelCase);
			Assert.AreEqual(false, settings.IgnoreNullValues);
			Assert.AreEqual(false, settings.IgnoreReadOnly);
			Assert.AreEqual(false, settings.IgnoreVirtuals);
			Assert.AreEqual(false, settings.ConvertEnumsToString);
		}

		[TestMethod]
		public void SettingsResetBackToDefaults()
		{
			ResetSettings(true, false, false, false, false, false);
			ResetSettings(false, true, false, false, false, false);
			ResetSettings(false, false, true, false, false, false);
			ResetSettings(false, false, false, true, false, false);
			ResetSettings(false, false, false, false, true, false);
			ResetSettings(false, false, false, false, false, true);
			ResetSettings(true, true, true, true, true, true);
		}

		private void ResetSettings(bool indented, bool camelCase, bool ignoreNullValues, bool ignoreReadOnly, bool ignoreVirtuals, bool convertEnumsToString)
		{
			SerializerSettings.DefaultSettings.Indented = indented;
			SerializerSettings.DefaultSettings.CamelCase = camelCase;
			SerializerSettings.DefaultSettings.IgnoreNullValues = ignoreNullValues;
			SerializerSettings.DefaultSettings.IgnoreReadOnly = ignoreReadOnly;
			SerializerSettings.DefaultSettings.IgnoreVirtuals = ignoreVirtuals;
			SerializerSettings.DefaultSettings.ConvertEnumsToString = convertEnumsToString;

			var settings = new SerializerSettings(!indented, !camelCase, !ignoreNullValues, !ignoreReadOnly, !ignoreVirtuals, !convertEnumsToString);
			Assert.AreEqual(settings.Indented, !indented);
			Assert.AreEqual(settings.CamelCase, !camelCase);
			Assert.AreEqual(settings.IgnoreNullValues, !ignoreNullValues);
			Assert.AreEqual(settings.IgnoreReadOnly, !ignoreReadOnly);
			Assert.AreEqual(settings.IgnoreVirtuals, !ignoreVirtuals);
			Assert.AreEqual(settings.ConvertEnumsToString, !convertEnumsToString);

			// Reset should reset to DefaultSettings
			settings.Reset();

			Assert.AreEqual(settings.Indented, indented);
			Assert.AreEqual(settings.CamelCase, camelCase);
			Assert.AreEqual(settings.IgnoreNullValues, ignoreNullValues);
			Assert.AreEqual(settings.IgnoreReadOnly, ignoreReadOnly);
			Assert.AreEqual(settings.IgnoreVirtuals, ignoreVirtuals);
			Assert.AreEqual(settings.ConvertEnumsToString, convertEnumsToString);

			// Reset the DefaultSettings to Speedy original defaults
			SerializerSettings.ResetDefaultSettings();
			settings.Reset();

			Assert.AreEqual(false, settings.Indented);
			Assert.AreEqual(false, settings.CamelCase);
			Assert.AreEqual(false, settings.IgnoreNullValues);
			Assert.AreEqual(false, settings.IgnoreReadOnly);
			Assert.AreEqual(false, settings.IgnoreVirtuals);
			Assert.AreEqual(false, settings.ConvertEnumsToString);
		}

		private void ValidateSettings(bool indented, bool camelCase, bool ignoreNullValues, bool ignoreReadOnly, bool ignoreVirtuals, bool convertEnumsToString)
		{
			var settings = new SerializerSettings(indented, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString);
			Assert.AreEqual(indented, settings.Indented);
			Assert.AreEqual(camelCase, settings.CamelCase);
			Assert.AreEqual(ignoreNullValues, settings.IgnoreNullValues);
			Assert.AreEqual(ignoreReadOnly, settings.IgnoreReadOnly);
			Assert.AreEqual(ignoreVirtuals, settings.IgnoreVirtuals);
			Assert.AreEqual(convertEnumsToString, settings.ConvertEnumsToString);
		}

		#endregion
	}
}