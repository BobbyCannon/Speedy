#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Speedy.Extensions;

#endregion

namespace Speedy.EntityFramework.Sql
{
	/// <summary>
	/// SQL builder to generate SQL scripts from Entity Framework queries. Supports both MSSQL and Sqlite.
	/// </summary>
	public static class SqlBuilder
	{
		#region Fields

		private static readonly Dictionary<Type, SqlDbType> _typeToSqlDbTypeDictionary;

		#endregion

		#region Constructors

		static SqlBuilder()
		{
			_typeToSqlDbTypeDictionary = new Dictionary<Type, SqlDbType>
			{
				{ typeof(long), SqlDbType.BigInt },
				{ typeof(long?), SqlDbType.BigInt },
				{ typeof(int), SqlDbType.Int },
				{ typeof(int?), SqlDbType.Int },
				{ typeof(byte), SqlDbType.TinyInt },
				{ typeof(byte?), SqlDbType.TinyInt },
				{ typeof(DateTime), SqlDbType.DateTime },
				{ typeof(DateTime?), SqlDbType.DateTime },
				{ typeof(bool), SqlDbType.Bit },
				{ typeof(bool?), SqlDbType.Bit },
				{ typeof(decimal), SqlDbType.Decimal },
				{ typeof(decimal?), SqlDbType.Decimal },
				{ typeof(double), SqlDbType.Float },
				{ typeof(double?), SqlDbType.Float },
				{ typeof(Guid), SqlDbType.UniqueIdentifier },
				{ typeof(Guid?), SqlDbType.UniqueIdentifier },
				{ typeof(string), SqlDbType.NVarChar }
			};
		}

		#endregion

		#region Methods

		/// <summary>
		/// Get SQL update script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> </param>
		/// <param name="query"> </param>
		/// <returns> </returns>
		public static (string, List<object>) GetSqlDelete<T>(EntityFrameworkDatabase database, IQueryable<T> query) where T : class
		{
			var (tableAlias, tableName, isLite) = GetBatchSql(query);
			var sqlWhere = new StringBuilder();
			var sqlParameters = new List<object>();
			var columnNames = SqlTableInformation.CreateInstance<T>(database).PropertyColumnNames;

			CreateWhere(columnNames, tableAlias, query.Expression, ref sqlWhere, ref sqlParameters, isLite);

			if (sqlWhere.Length <= 0)
			{
				throw new ArgumentException("Must have a filter query.", nameof(query));
			}

			return (isLite
				? $"DELETE FROM \"{tableAlias}\"{sqlWhere}"
				: RemoveAlias($"DELETE {tableName}{sqlWhere}", tableAlias), sqlParameters);
		}

		/// <summary>
		/// Get SQL update script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> </param>
		/// <param name="query"> </param>
		/// <param name="expression"> </param>
		/// <returns> </returns>
		public static (string, List<object>) GetSqlUpdate<T>(EntityFrameworkDatabase database, IQueryable<T> query, Expression<Func<T, T>> expression) where T : class
		{
			var (tableAlias, tableName, isLite) = GetBatchSql(query);
			var sqlColumns = new StringBuilder();
			var sqlWhere = new StringBuilder();
			var sqlParameters = new List<object>();
			var columnNames = SqlTableInformation.CreateInstance<T>(database).PropertyColumnNames;

			CreateUpdateBody(columnNames, tableAlias, expression.Body, ref sqlColumns, ref sqlParameters, isLite);
			CreateWhere(columnNames, tableAlias, query.Expression, ref sqlWhere, ref sqlParameters, isLite);

			return (isLite
				? $"UPDATE \"{tableAlias}\" SET{sqlColumns}{sqlWhere}"
				: $"UPDATE [{tableAlias}] SET{sqlColumns} {tableName}{sqlWhere}", sqlParameters);
		}

		private static void CreateUpdateBody(IReadOnlyDictionary<string, string> columnNames, string tableAlias, Expression expression, ref StringBuilder sqlColumns, ref List<object> sqlParameters, bool isLite)
		{
			switch (expression)
			{
				case MemberInitExpression memberInitExpression:
				{
					foreach (var item in memberInitExpression.Bindings)
					{
						if (!(item is MemberAssignment assignment))
						{
							continue;
						}

						sqlColumns.Append(columnNames.TryGetValue(assignment.Member.Name, out var value)
							? isLite ? $" \"{value}\"" : $" [{tableAlias}].[{value}]"
							: isLite
								? $" \"{assignment.Member.Name}\""
								: $" [{tableAlias}].[{assignment.Member.Name}]");
						sqlColumns.Append(" =");

						CreateUpdateBody(columnNames, tableAlias, assignment.Expression, ref sqlColumns, ref sqlParameters, isLite);

						if (memberInitExpression.Bindings.IndexOf(item) < memberInitExpression.Bindings.Count - 1)
						{
							sqlColumns.Append(" ,");
						}
					}
					break;
				}
				case MemberExpression memberExpression when memberExpression.Expression is ParameterExpression:
				{
					sqlColumns.Append(columnNames.TryGetValue(memberExpression.Member.Name, out var value)
						? isLite ? $" \"{value}\"" : $" [{tableAlias}].[{value}]"
						: isLite
							? $" \"{memberExpression.Member.Name}\""
							: $" [{tableAlias}].[{memberExpression.Member.Name}]");
					break;
				}
				case ConstantExpression constantExpression:
				{
					var name = $"param_{sqlParameters.Count}";
					sqlParameters.Add(isLite ? (object) new SqliteParameter(name, constantExpression.Value) : new SqlParameter(name, constantExpression.Value));
					sqlColumns.Append($" @{name}");
					break;
				}
				case UnaryExpression unaryExpression:
				{
					switch (unaryExpression.NodeType)
					{
						case ExpressionType.Convert:
							CreateUpdateBody(columnNames, tableAlias, unaryExpression.Operand, ref sqlColumns, ref sqlParameters, isLite);
							break;

						case ExpressionType.Not:
							sqlColumns.Append(" ~");
							CreateUpdateBody(columnNames, tableAlias, unaryExpression.Operand, ref sqlColumns, ref sqlParameters, isLite);
							break;
					}
					break;
				}
				case BinaryExpression binaryExpression:
				{
					var valueIsNull = binaryExpression.Right is ConstantExpression constantExpression && constantExpression.Value == null;
					CreateUpdateBody(columnNames, tableAlias, binaryExpression.Left, ref sqlColumns, ref sqlParameters, isLite);
					sqlColumns.Append(GetNodeType(binaryExpression, valueIsNull));
					if (!valueIsNull)
					{
						CreateUpdateBody(columnNames, tableAlias, binaryExpression.Right, ref sqlColumns, ref sqlParameters, isLite);
					}
					break;
				}
				default:
				{
					var value = Expression.Lambda(expression).Compile().DynamicInvoke();
					var parameterName = $"param_{sqlParameters.Count}";
					sqlParameters.Add(isLite ? (object) new SqliteParameter(parameterName, value) : new SqlParameter(parameterName, value));
					sqlColumns.Append($" @{parameterName}");
					break;
				}
			}
		}

		private static void CreateWhere(IReadOnlyDictionary<string, string> columnNames, string tableAlias, Expression expression, ref StringBuilder sqlWhere, ref List<object> sqlParameters, bool isLite)
		{
			switch (expression)
			{
				case MemberExpression memberExpression:
				{
					var result = Process(memberExpression, isLite, sqlWhere, sqlParameters);
					if (!result)
					{
						sqlWhere.Append(columnNames.TryGetValue(memberExpression.Member.Name, out var value)
							? isLite ? $" \"{value}\"" : $" [{tableAlias}].[{value}]"
							: isLite
								? $" \"{memberExpression.Member.Name}\""
								: $" [{tableAlias}].[{memberExpression.Member.Name}]");
					}
					break;
				}
				case ConstantExpression constantExpression:
				{
					if (sqlWhere.Length <= 0)
					{
						return;
					}

					var name = $"param_{sqlParameters.Count}";
					sqlParameters.Add(isLite
						? (object) new SqliteParameter(name, GetSqlType(constantExpression)) { Value = constantExpression.Value }
						: new SqlParameter(name, GetSqlType(constantExpression)) { Value = constantExpression.Value }
					);
					sqlWhere.Append($" @{name}");
					break;
				}
				case UnaryExpression unaryExpression:
				{
					switch (unaryExpression.NodeType)
					{
						case ExpressionType.Convert:
						case ExpressionType.Quote:
							var body = unaryExpression.Operand.GetMemberValue("Body") as Expression;
							CreateWhere(columnNames, tableAlias, body ?? unaryExpression.Operand, ref sqlWhere, ref sqlParameters, isLite);
							break;

						case ExpressionType.Not:
							CreateWhere(columnNames, tableAlias, unaryExpression.Operand, ref sqlWhere, ref sqlParameters, isLite);
							sqlWhere.Append(" = 0");
							break;
					}
					break;
				}
				case BinaryExpression binaryExpression:
				{
					var valueIsNull = binaryExpression.Right is ConstantExpression constantExpression && constantExpression.Value == null;
					CreateWhere(columnNames, tableAlias, binaryExpression.Left, ref sqlWhere, ref sqlParameters, isLite);
					sqlWhere.Append(GetNodeType(binaryExpression, valueIsNull));
					if (!valueIsNull)
					{
						CreateWhere(columnNames, tableAlias, binaryExpression.Right, ref sqlWhere, ref sqlParameters, isLite);
					}
					break;
				}
				case MethodCallExpression methodExpression:
				{
					if (sqlWhere.Length <= 0)
					{
						sqlWhere.Append(" WHERE");
					}

					var offset = methodExpression.Arguments.Count == 1 ? 0 : 1;
					CreateWhere(columnNames, tableAlias, methodExpression.Arguments[offset], ref sqlWhere, ref sqlParameters, isLite);
					break;
				}
				default:
				{
					// Need to 
					Debug.WriteLine(expression.GetType().FullName);
					break;
				}
			}
		}

		private static (string tableAlias, string tableName, bool isLite) GetBatchSql<T>(IQueryable<T> query) where T : class
		{
			var sqlQuery = query.ToSql();
			var isLite = false;

			// UPDATE [x]
			// UPDATE "x"
			// 0123456789
			var alias = sqlQuery.IndexOf("]", 8);
			if (alias == -1)
			{
				alias = sqlQuery.IndexOf("\"", 8);
				isLite = true;
			}

			var tableAlias = sqlQuery.Substring(8, alias - 8);
			var index = sqlQuery.IndexOf(Environment.NewLine);
			var sql = sqlQuery.Substring(index, sqlQuery.Length - index);
			var tableName = tableAlias;

			if (isLite)
			{
				tableAlias = sql.Substring(sql.IndexOf("\""));
				tableAlias = tableAlias.Substring(1, tableAlias.IndexOf("\"", 1) - 1);
			}
			else
			{
				sql = sql.Replace("\r\n", " ");
				var fromIndex = sql.IndexOf("from ", StringComparison.OrdinalIgnoreCase);
				var whereIndex = sql.IndexOf(" where ", StringComparison.OrdinalIgnoreCase);
				var length = whereIndex > fromIndex ? whereIndex - fromIndex : sql.Length - fromIndex;
				tableName = sql.Substring(fromIndex, length);
			}

			return (tableAlias, tableName, isLite);
		}

		private static string GetNodeType(BinaryExpression expression, bool valueIsNull)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.AndAlso:
					return " AND";

				case ExpressionType.OrElse:
					return " OR";

				case ExpressionType.Add:
					return " +";

				case ExpressionType.AddAssign:
					return " +=";

				case ExpressionType.Subtract:
					return " -";

				case ExpressionType.SubtractAssign:
					return " -=";

				case ExpressionType.And:
					return " &";

				case ExpressionType.AndAssign:
					return " &=";

				case ExpressionType.Or:
					return " |";

				case ExpressionType.Modulo:
					return " %";

				case ExpressionType.ModuloAssign:
					return " %=";

				case ExpressionType.Divide:
					return " /";

				case ExpressionType.DivideAssign:
					return " /=";

				case ExpressionType.Multiply:
					return " *";

				case ExpressionType.MultiplyAssign:
					return " *=";

				case ExpressionType.GreaterThan:
					return " >";

				case ExpressionType.GreaterThanOrEqual:
					return " >=";

				case ExpressionType.LessThan:
					return " <";

				case ExpressionType.LessThanOrEqual:
					return " <=";

				case ExpressionType.Assign:
				case ExpressionType.Equal:
					return valueIsNull ? " IS NULL" : " =";

				case ExpressionType.NotEqual:
					return valueIsNull ? " IS NOT NULL" : " <>";

				default:
					throw new NotSupportedException(expression.NodeType.ToString());
			}
		}

		private static SqlDbType GetSqlType(ConstantExpression expression)
		{
			if (_typeToSqlDbTypeDictionary.ContainsKey(expression.Type))
			{
				return _typeToSqlDbTypeDictionary[expression.Type];
			}
			
			return SqlDbType.BigInt;
		}

		private static bool Process(MemberExpression memberExpression, bool isLite, StringBuilder sqlWhere, List<object> sqlParameters)
		{
			if (memberExpression.Expression != null && !(memberExpression.Expression is ConstantExpression))
			{
				return false;
			}

			var name = $"param_{sqlParameters.Count}";
			object parameter;

			if (memberExpression.Member.DeclaringType == typeof(DateTime))
			{
				var value = memberExpression.Member.Name switch
				{
					nameof(DateTime.MinValue) => DateTime.MinValue,
					nameof(DateTime.MaxValue) => DateTime.MaxValue,
					_ => (object) null
				};
				parameter = isLite ? (object) new SqliteParameter(name, value) : new SqlParameter(name, SqlDbType.DateTime2) { Value = value };
			}
			else
			{
				var value = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
				parameter = isLite ? (object) new SqliteParameter(name, value) : new SqlParameter(name, value);
			}

			sqlParameters.Add(parameter);
			sqlWhere.Append($" @{name}");

			return true;
		}

		private static string RemoveAlias(string query, string alias)
		{
			return query.Replace($" AS [{alias}]", string.Empty)
				.Replace($"[{alias}].", string.Empty);
		}

		#endregion
	}
}