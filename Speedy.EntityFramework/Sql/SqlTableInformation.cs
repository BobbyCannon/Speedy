#region References

using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.EntityFramework.Sql
{
	internal class SqlTableInformation
	{
		#region Fields

		private Dictionary<string, string> _propertyToColumnNames;

		#endregion

		#region Constructors

		private SqlTableInformation()
		{
			EntityProperties = new Dictionary<string, PropertyInfo>();
			Properties = new List<IProperty>();
			TableName = string.Empty;
		}

		#endregion

		#region Properties

		public IReadOnlyDictionary<string, PropertyInfo> EntityProperties { get; private set; }

		public IReadOnlyList<IProperty> PrimaryKeys { get; private set; }

		public IReadOnlyList<IProperty> Properties { get; private set; }

		public IDictionary<string, string> PropertyToColumnName => _propertyToColumnNames ??= Properties.ToDictionary(x => x.Name, x => x.GetColumnName());

		public string ProviderPrefix { get; set; }

		public string ProviderSuffix { get; set; }

		public DatabaseProviderType ProviderType { get; set; }

		public string SchemaName { get; private set; }

		public IProperty SyncKey { get; private set; }

		public string TableName { get; private set; }

		#endregion

		#region Methods

		public static SqlTableInformation CreateInstance<T>(EntityFrameworkDatabase database)
		{
			var tableInfo = new SqlTableInformation();
			tableInfo.LoadData<T>(database);
			return tableInfo;
		}

		public string GetFormattedTableName()
		{
			return ProviderType == DatabaseProviderType.Sqlite
				? $"{ProviderPrefix}{TableName}{ProviderSuffix}"
				: $"{ProviderPrefix}{SchemaName}{ProviderSuffix}.{ProviderPrefix}{TableName}{ProviderSuffix}";
		}

		private void LoadData<T>(EntityFrameworkDatabase database)
		{
			// todo: add caching for speed

			var type = typeof(T);
			var entityType = database.Model.FindEntityType(type);
			var allProperties = entityType.GetProperties().OrderBy(x => x.Name).ToList();
			var timestampDbTypeName = nameof(TimestampAttribute).Replace("Attribute", "").ToLower();
			var timeStampProperties = allProperties.Where(a => a.IsConcurrencyToken && a.ValueGenerated == ValueGenerated.OnAddOrUpdate || a.GetColumnType() == timestampDbTypeName).ToList();
			var allPropertiesExceptTimeStamp = allProperties.Except(timeStampProperties).ToList();

			// Prepare for updates
			var entityProperties = type.GetCachedProperties().ToList();
			var properties = allPropertiesExceptTimeStamp
				.Where(a => a.GetComputedColumnSql() == null)
				.OrderBy(x => x.GetColumnName())
				.ToList();

			// Update members with new values
			TableName = entityType.GetTableName();
			SchemaName = entityType.GetSchema();
			PrimaryKeys = entityType.FindPrimaryKey().Properties;
			Properties = properties.ToImmutableList();
			ProviderType = database.GetProviderType();
			ProviderPrefix = ProviderType == DatabaseProviderType.Sqlite ? "\"" : "[";
			ProviderSuffix = ProviderType == DatabaseProviderType.Sqlite ? "\"" : "]";
			SyncKey = properties.FirstOrDefault(x => x.PropertyInfo.Name == nameof(ISyncEntity.SyncId));
			EntityProperties = properties.ToImmutableDictionary(x => x.Name, x => entityProperties.FirstOrDefault(p => p.Name == x.Name));
		}

		#endregion
	}
}