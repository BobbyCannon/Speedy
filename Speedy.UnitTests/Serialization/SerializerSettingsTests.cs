#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Serialization;

#endregion

namespace Speedy.UnitTests.Serialization
{
	[TestClass]
	public class SerializerSettingsTests
	{
		#region Methods

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