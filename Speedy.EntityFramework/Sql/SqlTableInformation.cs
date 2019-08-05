#region References

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#endregion

namespace Speedy.EntityFramework.Sql
{
	internal class SqlTableInformation
	{
		#region Constructors

		private SqlTableInformation()
		{
			PropertyColumnNames = new Dictionary<string, string>();
		}

		#endregion

		#region Properties

		public Dictionary<string, string> PropertyColumnNames { get; private set; }

		#endregion

		#region Methods

		public static SqlTableInformation CreateInstance<T>(DbContext context)
		{
			var tableInfo = new SqlTableInformation();
			tableInfo.LoadData<T>(context);
			return tableInfo;
		}

		private void LoadData<T>(DbContext context)
		{
			var type = typeof(T);
			var entityType = context.Model.FindEntityType(type);
			var allProperties = entityType.GetProperties().ToList();
			var timestampDbTypeName = nameof(TimestampAttribute).Replace("Attribute", "").ToLower();
			var timeStampProperties = allProperties.Where(a => a.IsConcurrencyToken && a.ValueGenerated == ValueGenerated.OnAddOrUpdate || a.Relational().ColumnType == timestampDbTypeName).ToList();
			var allPropertiesExceptTimeStamp = allProperties.Except(timeStampProperties).ToList();
			var properties = allPropertiesExceptTimeStamp.Where(a => a.Relational().ComputedColumnSql == null).ToList();

			PropertyColumnNames = properties.ToDictionary(a => a.Name, b => b.Relational().ColumnName.Replace("]", "]]"));
		}

		#endregion
	}
}