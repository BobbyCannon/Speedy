#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Speedy.Extensions;

#endregion

namespace Speedy.EntityFramework.Sql
{
	/// <summary>
	/// Represents a SQL statement
	/// </summary>
	public class SqlStatement
	{
		#region Fields

		private static readonly Dictionary<SqlDbType, SqliteType> _sqlDbTypeToSqliteTypeDictionary;
		private static readonly Dictionary<Type, SqlDbType> _typeToSqlDbTypeDictionary;
		private static readonly Dictionary<Type, SqliteType> _typeToSqliteTypeDictionary;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of a SQL statement
		/// </summary>
		internal SqlStatement(SqlTableInformation tableInformation)
		{
			TableInformation = tableInformation;
			ParameterPrefix = "p";
			Parameters = new List<object>();
			ParametersByColumnName = new Dictionary<string, object>();
			ParameterNameByColumnName = new Dictionary<string, string>();
			Query = new StringBuilder();
			QueryWhere = new StringBuilder();
			QueryUpdate = new StringBuilder();
		}

		static SqlStatement()
		{
			// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
			_typeToSqlDbTypeDictionary = new Dictionary<Type, SqlDbType>
			{
				{ typeof(bool), SqlDbType.Bit },
				{ typeof(bool?), SqlDbType.Bit },
				{ typeof(byte), SqlDbType.TinyInt },
				{ typeof(byte?), SqlDbType.TinyInt },
				{ typeof(sbyte), SqlDbType.TinyInt },
				{ typeof(sbyte?), SqlDbType.TinyInt },
				{ typeof(byte[]), SqlDbType.VarBinary },
				{ typeof(char), SqlDbType.Char },
				{ typeof(char?), SqlDbType.Char },
				{ typeof(DateTime), SqlDbType.DateTime2 },
				{ typeof(DateTime?), SqlDbType.DateTime2 },
				{ typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
				{ typeof(DateTimeOffset?), SqlDbType.DateTimeOffset },
				{ typeof(TimeSpan), SqlDbType.Time },
				{ typeof(TimeSpan?), SqlDbType.Time },
				{ typeof(decimal), SqlDbType.Decimal },
				{ typeof(decimal?), SqlDbType.Decimal },
				{ typeof(double), SqlDbType.Float },
				{ typeof(double?), SqlDbType.Float },
				{ typeof(float), SqlDbType.Float },
				{ typeof(float?), SqlDbType.Float },
				{ typeof(Guid), SqlDbType.UniqueIdentifier },
				{ typeof(Guid?), SqlDbType.UniqueIdentifier },
				{ typeof(short), SqlDbType.SmallInt },
				{ typeof(short?), SqlDbType.SmallInt },
				{ typeof(ushort), SqlDbType.SmallInt },
				{ typeof(ushort?), SqlDbType.SmallInt },
				{ typeof(int), SqlDbType.Int },
				{ typeof(int?), SqlDbType.Int },
				{ typeof(uint), SqlDbType.Int },
				{ typeof(uint?), SqlDbType.Int },
				{ typeof(long), SqlDbType.BigInt },
				{ typeof(long?), SqlDbType.BigInt },
				{ typeof(ulong), SqlDbType.BigInt },
				{ typeof(ulong?), SqlDbType.BigInt },
				{ typeof(string), SqlDbType.Text },
				{ typeof(object), SqlDbType.Variant }
			};

			// https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/types
			_typeToSqliteTypeDictionary = new Dictionary<Type, SqliteType>
			{
				{ typeof(bool), SqliteType.Integer },
				{ typeof(bool?), SqliteType.Integer },
				{ typeof(byte), SqliteType.Integer },
				{ typeof(byte?), SqliteType.Integer },
				{ typeof(sbyte), SqliteType.Integer },
				{ typeof(sbyte?), SqliteType.Integer },
				{ typeof(byte[]), SqliteType.Blob },
				{ typeof(char), SqliteType.Text },
				{ typeof(char?), SqliteType.Text },
				{ typeof(DateTime), SqliteType.Text },
				{ typeof(DateTime?), SqliteType.Text },
				{ typeof(DateTimeOffset), SqliteType.Text },
				{ typeof(DateTimeOffset?), SqliteType.Text },
				{ typeof(TimeSpan), SqliteType.Text },
				{ typeof(TimeSpan?), SqliteType.Text },
				{ typeof(decimal), SqliteType.Text },
				{ typeof(decimal?), SqliteType.Text },
				{ typeof(double), SqliteType.Real },
				{ typeof(double?), SqliteType.Real },
				{ typeof(float), SqliteType.Real },
				{ typeof(float?), SqliteType.Real },
				{ typeof(Guid), SqliteType.Text },
				{ typeof(Guid?), SqliteType.Text },
				{ typeof(short), SqliteType.Integer },
				{ typeof(short?), SqliteType.Integer },
				{ typeof(ushort), SqliteType.Integer },
				{ typeof(ushort?), SqliteType.Integer },
				{ typeof(int), SqliteType.Integer },
				{ typeof(int?), SqliteType.Integer },
				{ typeof(uint), SqliteType.Integer },
				{ typeof(uint?), SqliteType.Integer },
				{ typeof(long), SqliteType.Integer },
				{ typeof(long?), SqliteType.Integer },
				{ typeof(ulong), SqliteType.Integer },
				{ typeof(ulong?), SqliteType.Integer },
				{ typeof(string), SqliteType.Text },
				{ typeof(object), SqliteType.Text }
			};

			_sqlDbTypeToSqliteTypeDictionary = new Dictionary<SqlDbType, SqliteType>
			{
				{ SqlDbType.BigInt, SqliteType.Integer },
				{ SqlDbType.Binary, SqliteType.Blob },
				{ SqlDbType.Bit, SqliteType.Integer },
				{ SqlDbType.Char, SqliteType.Text },
				{ SqlDbType.Date, SqliteType.Text },
				{ SqlDbType.DateTime, SqliteType.Text },
				{ SqlDbType.DateTime2, SqliteType.Text },
				{ SqlDbType.DateTimeOffset, SqliteType.Text },
				{ SqlDbType.Decimal, SqliteType.Real },
				{ SqlDbType.Float, SqliteType.Real },
				{ SqlDbType.Image, SqliteType.Blob },
				{ SqlDbType.Int, SqliteType.Integer },
				{ SqlDbType.Money, SqliteType.Real },
				{ SqlDbType.NChar, SqliteType.Text },
				{ SqlDbType.NText, SqliteType.Text },
				{ SqlDbType.NVarChar, SqliteType.Text },
				{ SqlDbType.Real, SqliteType.Real },
				{ SqlDbType.SmallDateTime, SqliteType.Text },
				{ SqlDbType.SmallInt, SqliteType.Integer },
				{ SqlDbType.SmallMoney, SqliteType.Real },
				{ SqlDbType.Structured, SqliteType.Text },
				{ SqlDbType.Text, SqliteType.Text },
				{ SqlDbType.Time, SqliteType.Text },
				{ SqlDbType.Timestamp, SqliteType.Text },
				{ SqlDbType.TinyInt, SqliteType.Integer },
				{ SqlDbType.Udt, SqliteType.Text },
				{ SqlDbType.UniqueIdentifier, SqliteType.Text },
				{ SqlDbType.VarBinary, SqliteType.Blob },
				{ SqlDbType.VarChar, SqliteType.Text },
				{ SqlDbType.Variant, SqliteType.Text },
				{ SqlDbType.Xml, SqliteType.Text }
			};
		}

		#endregion

		#region Properties

		/// <summary>
		/// Locate a parameter name by the column name. Ex. Parameters may be added as p0, p1 but for p0 == Id, and p1 == Name.
		/// </summary>
		public IDictionary<string, string> ParameterNameByColumnName { get; }

		/// <summary>
		/// The parameter prefixes.
		/// </summary>
		public string ParameterPrefix { get; set; }

		/// <summary>
		/// Parameter values
		/// </summary>
		public IList<object> Parameters { get; }

		/// <summary>
		/// Locate a parameter by the column name. Ex. Parameters may be added as p0, p1 but for p0 == Id, and p1 == Name.
		/// </summary>
		public IDictionary<string, object> ParametersByColumnName { get; }

		/// <summary>
		/// The query generated by SQL builder.
		/// </summary>
		public StringBuilder Query { get; }

		/// <summary>
		/// The optional update portion of the query.
		/// </summary>
		public StringBuilder QueryUpdate { get; }

		/// <summary>
		/// The optional where portion of the query.
		/// </summary>
		public StringBuilder QueryWhere { get; }

		/// <summary>
		/// The table information.
		/// </summary>
		internal SqlTableInformation TableInformation { get; }

		#endregion

		#region Methods

		internal string AddOrUpdateParameter(string columnName, SqlDbType type)
		{
			return AddOrUpdateParameterValue(columnName, type, null);
		}

		internal string AddOrUpdateParameterValue(string columnName, SqlDbType type, object value)
		{
			if (!ParameterNameByColumnName.TryGetValue(columnName, out var parameterName))
			{
				parameterName = GetNextParameterName();
			}

			switch (TableInformation.ProviderType)
			{
				case DatabaseProviderType.Sqlite:
				{
					var parameter = Parameters.Cast<SqliteParameter>().FirstOrDefault(x => x.ParameterName == parameterName);
					if (parameter == null)
					{
						parameter = new SqliteParameter(parameterName, GetSqliteType(type)) { Value = value };
						Parameters.Add(parameter);
						ParametersByColumnName.Add(columnName, parameter);
						ParameterNameByColumnName.Add(columnName, parameter.ParameterName);
					}
					else
					{
						parameter.SqliteType = GetSqliteType(type);
						parameter.Value = value;
					}
					break;
				}
				case DatabaseProviderType.SqlServer:
				{
					var parameter = Parameters.Cast<SqlParameter>().FirstOrDefault(x => x.ParameterName == parameterName);
					if (parameter == null)
					{
						parameter = new SqlParameter(parameterName, type) { Value = value };
						Parameters.Add(parameter);
						ParametersByColumnName.Add(columnName, parameter);
						ParameterNameByColumnName.Add(columnName, parameter.ParameterName);
					}
					else
					{
						parameter.SqlDbType = type;
						parameter.Value = value;
					}
					break;
				}
				case DatabaseProviderType.Unknown:
				default:
					throw new NotSupportedException();
			}

			return parameterName;
		}

		internal string AddParameterValue(string columnName, ConstantExpression expression)
		{
			return AddParameterValue(columnName, GetSqlType(expression.Type), expression.Value);
		}

		internal string AddParameterValue(string columnName, MemberExpression expression)
		{
			return AddParameterValue(columnName, GetSqlType(expression.Type), GetValue(expression));
		}

		internal string AddParameterValue(string columnName, object value)
		{
			return AddParameterValue(columnName, GetSqlType(value.GetType()), value);
		}

		internal string AddParameterValue(string columnName, SqlDbType type, object value)
		{
			var name = GetNextParameterName();

			switch (TableInformation.ProviderType)
			{
				case DatabaseProviderType.Sqlite:
				{
					var parameter = new SqliteParameter(name, type) { Value = value };
					Parameters.Add(parameter);

					if ((columnName != null) && !ParametersByColumnName.ContainsKey(columnName))
					{
						ParametersByColumnName.Add(columnName, parameter);
						ParameterNameByColumnName.Add(columnName, parameter.ParameterName);
					}
					break;
				}
				case DatabaseProviderType.SqlServer:
				{
					var parameter = new SqlParameter(name, type) { Value = value };
					Parameters.Add(parameter);

					if ((columnName != null) && !ParametersByColumnName.ContainsKey(columnName))
					{
						ParametersByColumnName.Add(columnName, parameter);
						ParameterNameByColumnName.Add(columnName, parameter.ParameterName);
					}
					break;
				}
				case DatabaseProviderType.Unknown:
				default:
					throw new NotSupportedException();
			}

			return name;
		}

		internal string GetDelimitedColumnNameList(IEnumerable<string> columnNames)
		{
			return TableInformation.ProviderPrefix
				+ string.Join($"{TableInformation.ProviderSuffix}, {TableInformation.ProviderPrefix}", columnNames)
				+ TableInformation.ProviderSuffix;
		}

		internal string GetSetColumnList(IDictionary<string, string> columnsAndParameters)
		{
			return string.Join(", ", columnsAndParameters.Select(x => $"{TableInformation.ProviderPrefix}{x.Key}{TableInformation.ProviderSuffix} = @{x.Value}"));
		}

		internal IDictionary<string, string> GetSqlColumnParameterNames(bool includePrimaryKeys = false, bool onlyIncludePrimaryKeys = false, params string[] excludedColumns)
		{
			if (onlyIncludePrimaryKeys)
			{
				return TableInformation.Properties
					.Where(x => x.IsPrimaryKey() && !excludedColumns.Contains(x.GetColumnName()))
					.Select(x =>
					{
						var dbType = GetSqlType(x.PropertyInfo.PropertyType);
						var columnName = x.GetColumnName();
						var parameterName = AddOrUpdateParameter(columnName, dbType);
						return (columnName, parameterName);
					})
					.ToDictionary(x => x.columnName, x => x.parameterName);
			}

			TableInformation.Properties
				.Where(x => !excludedColumns.Contains(x.GetColumnName())
					&& (!x.IsPrimaryKey() || (includePrimaryKeys && x.IsPrimaryKey())))
				.ForEach(x =>
				{
					var dbType = GetSqlType(x.PropertyInfo.PropertyType);
					AddOrUpdateParameter(x.GetColumnName(), dbType);
				});

			return ParameterNameByColumnName;
		}

		internal static SqliteType GetSqliteType(SqlDbType type)
		{
			if (_sqlDbTypeToSqliteTypeDictionary.TryGetValue(type, out var sqliteType))
			{
				return sqliteType;
			}
			return SqliteType.Integer;
		}

		internal static SqliteType GetSqliteType(Type type)
		{
			return _typeToSqliteTypeDictionary.TryGetValue(type, out var sqliteType) ? sqliteType : SqliteType.Integer;
		}

		internal static SqlDbType GetSqlType(Type type)
		{
			if (_typeToSqlDbTypeDictionary.ContainsKey(type))
			{
				return _typeToSqlDbTypeDictionary[type];
			}
			return SqlDbType.BigInt;
		}

		internal static object GetValue(MemberExpression member)
		{
			var objectMember = Expression.Convert(member, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();
			return getter();
		}

		internal string GetWhereColumnList(IDictionary<string, string> columnsAndParameters)
		{
			return string.Join(" AND ", columnsAndParameters.Select(x => $"{TableInformation.ProviderPrefix}{x.Key}{TableInformation.ProviderSuffix} = @{x.Value}"));
		}

		private string GetNextParameterName()
		{
			return $"{ParameterPrefix}{Parameters.Count}";
		}

		#endregion
	}
}