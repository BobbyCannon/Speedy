#region References

using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.EntityFramework.Mappings;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class MappingConverterTests
	{
		#region Methods

		/// <summary>
		/// Test research on if we can convert an Entity Map into Speedy Memory Database configuration.
		/// I'd like to reduce the amount of dual configuration needed so developers would not have to dual
		/// configure their entities.
		/// </summary>
		[TestMethod]
		public void ConvertMapIntoMemoryEntityConfiguration()
		{
			var test = new AddressMap();
			var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			var properties = test.GetCachedProperties(flags);
			properties.Count.Dump();
			properties.ForEach(x => x.Name.Dump());
			Debug.WriteLine("");

			var configurationProperty = properties.First(x => x.Name == "Configuration");
			var configuration = configurationProperty.GetValue(test);
			properties = configuration.GetCachedProperties(flags);
			properties.Count.Dump();
			properties.ForEach(x => x.Name.Dump());
			Debug.WriteLine(" /properties");
			Debug.WriteLine("");

			var fields = configuration.GetCachedFields(flags);
			fields.Count.Dump();
			fields.ForEach(x => (x.Name + " - " + x.FieldType).Dump());
			Debug.WriteLine("  /fields");
			Debug.WriteLine("");

			//var tableName = properties.First(x => x.Name == "TableName").GetValue(configuration);
			//tableName.Dump();

			//var configurationProperties = (IEnumerable) properties.First(x => x.Name == "ConfiguredProperties").GetValue(configuration);
			//configurationProperties.ForEach(x => ((PropertyInfo) x).Name.Dump());

			var entityMappingConfigurations = (IDictionary) properties.First(x => x.Name == "PrimitivePropertyConfigurations").GetValue(configuration);
			entityMappingConfigurations.ForEach(m =>
			{
				var valueType = m.GetType();
				var kvpKey = valueType.GetProperty("Key")?.GetValue(m, null);
				var kvpValue = valueType.GetProperty("Value")?.GetValue(m, null);
				kvpKey.Dump();
				kvpValue.Dump();
				properties = kvpValue.GetCachedProperties(flags);
				properties.Count.Dump();
				properties.ForEach(x => ("\t" + x.Name).Dump());
				var columnName = properties.First(x => x.Name == "ColumnName").GetValue(kvpValue);
				columnName.Dump();

				var columnType = properties.First(x => x.Name == "ColumnType").GetValue(kvpValue);
				columnType.Dump();

				var columnOrder = properties.First(x => x.Name == "ColumnOrder").GetValue(kvpValue);
				columnOrder.Dump();

				Debug.WriteLine("");
				Debug.WriteLine("");
			});

			test.ToString().Dump();
		}

		#endregion
	}
}