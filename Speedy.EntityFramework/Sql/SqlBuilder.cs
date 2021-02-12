#region References

using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Speedy.Extensions;

#endregion

namespace Speedy.EntityFramework.Sql
{
	/// <summary>
	/// SQL builder to generate SQL scripts from Entity Framework queries. Supports both MSSQL and Sqlite.
	/// </summary>
	public static class SqlBuilder
	{
		#region Methods

		/// <summary>
		/// Get SQL delete script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> The database to process. </param>
		/// <param name="query"> The query to process. </param>
		/// <returns> The SQL script and values to be deleted. </returns>
		public static SqlStatement GetSqlDelete<T>(EntityFrameworkDatabase database, IQueryable<T> query) where T : class
		{
			var tableInformation = SqlTableInformation.CreateInstance<T>(database);
			var response = new SqlStatement(tableInformation);

			CreateWhere(response, query.Expression);

			if (response.QueryWhere.Length <= 0)
			{
				throw new ArgumentException("Must have a filter query.", nameof(query));
			}

			response.Query.Append(
				tableInformation.ProviderType == DatabaseProviderType.Sqlite
					? $"DELETE FROM \"{tableInformation.TableName}\"{response.QueryWhere}"
					: $"DELETE FROM [{tableInformation.SchemaName}].[{tableInformation.TableName}]{response.QueryWhere}"
			);

			return response;
		}

		/// <summary>
		/// Get SQL insert script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> The database to process. </param>
		/// <returns> The SQL insert script. </returns>
		public static SqlStatement GetSqlInsert<T>(EntityFrameworkDatabase database) where T : class
		{
			var tableInformation = SqlTableInformation.CreateInstance<T>(database);
			var statement = new SqlStatement(tableInformation);
			return GetSqlInsert(statement);
		}

		/// <summary>
		/// Get SQL insert script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> The database to process. </param>
		/// <param name="entity"> The entity to process. </param>
		/// <returns> The SQL script and values to be inserted. </returns>
		public static SqlStatement GetSqlInsert<T>(EntityFrameworkDatabase database, T entity) where T : class
		{
			var tableInformation = SqlTableInformation.CreateInstance<T>(database);
			var statement = new SqlStatement(tableInformation);
			var response = GetSqlInsert(statement);
			UpdateStatementParameters(statement, entity);
			return response;
		}

		/// <summary>
		/// Get SQL merge script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> The database to process. </param>
		/// <returns> The SQL merge script. </returns>
		public static SqlStatement GetSqlInsertOrUpdate<T>(EntityFrameworkDatabase database) where T : class
		{
			var tableInformation = SqlTableInformation.CreateInstance<T>(database);
			var response = new SqlStatement(tableInformation);
			var createdOnColumnName = tableInformation.PropertyToColumnName["CreatedOn"];
			var syncIdColumnName = tableInformation.PropertyToColumnName["SyncId"];

			switch (tableInformation.ProviderType)
			{
				case DatabaseProviderType.SqlServer:
				{
					response.Query.AppendLine("SET NOCOUNT, XACT_ABORT ON;");
					response.Query.AppendLine($"MERGE {response.TableInformation.GetFormattedTableName()} WITH (HOLDLOCK) AS T");
					response.Query.AppendFormat("USING (SELECT @[[SyncIdParameterName]] as {0}) AS S\r\n\tON T.[{0}] = S.[{0}]\r\nWHEN MATCHED\r\n\tTHEN ", syncIdColumnName);
					GetSqlUpdate(response, true, true, createdOnColumnName, syncIdColumnName);
					response.Query.AppendLine();
					response.Query.Append("WHEN NOT MATCHED\r\n\tTHEN ");
					GetSqlInsert(response, true);
					response.Query.Append(";");

					var syncIdParameterName = response.ParameterNameByColumnName[syncIdColumnName];
					response.Query.Replace("[[SyncIdParameterName]]", syncIdParameterName);
					break;
				}
				case DatabaseProviderType.Sqlite:
				{
					GetSqlUpdate(response, excludeWhere: true, excludedColumns: new[] { createdOnColumnName, syncIdColumnName });
					response.AddParameterValue(syncIdColumnName, SqlDbType.UniqueIdentifier, Guid.Empty);
					var syncIdParameterName = response.ParameterNameByColumnName[syncIdColumnName];
					var where = $" WHERE {tableInformation.ProviderPrefix}{syncIdColumnName}{tableInformation.ProviderSuffix} = @{syncIdParameterName}";
					response.Query.AppendLine($"{where};");
					GetSqlInsert(response);
					response.Query.AppendLine();
					response.Query.Append("WHERE (SELECT Changes() = 0);");
					response.Query.Replace(") VALUES (", ")\r\nSELECT ").Replace(")\r\nWHERE (SELECT", "\r\nWHERE (SELECT");
					break;
				}
			}

			return response;
		}

		/// <summary>
		/// Get SQL insert script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> The database to process. </param>
		/// <returns> The SQL update statement. </returns>
		public static SqlStatement GetSqlUpdate<T>(EntityFrameworkDatabase database) where T : class
		{
			var tableInformation = SqlTableInformation.CreateInstance<T>(database);
			var response = new SqlStatement(tableInformation);
			return GetSqlUpdate(response);
		}

		/// <summary>
		/// Get SQL update script from query.
		/// </summary>
		/// <typeparam name="T"> The type for the query. </typeparam>
		/// <param name="database"> The database to process. </param>
		/// <param name="query"> The query to process. </param>
		/// <param name="expression"> The expression to process. </param>
		/// <returns> The SQL update statement. </returns>
		public static SqlStatement GetSqlUpdate<T>(EntityFrameworkDatabase database, IQueryable<T> query, Expression<Func<T, T>> expression) where T : class
		{
			var tableInformation = SqlTableInformation.CreateInstance<T>(database);
			var response = new SqlStatement(tableInformation);
			string memberName = null;

			CreateUpdateBody(response, expression.Body, ref memberName);
			CreateWhere(response, query.Expression);

			response.Query.Append($"UPDATE {tableInformation.GetFormattedTableName()} SET{response.QueryUpdate}{response.QueryWhere}");

			return response;
		}

		internal static void UpdateCommand<T>(SqlStatement statement, SqlCommand command, T entity)
		{
			if (command.Parameters.Count <= 0)
			{
				command.Parameters.AddRange(statement.Parameters.Cast<SqlParameter>().ToArray());
			}

			foreach (var property in statement.TableInformation.Properties)
			{
				var entityProperty = statement.TableInformation.EntityProperties[property.Name];
				var columnName = property.GetColumnName();

				if (!statement.ParametersByColumnName.TryGetValue(columnName, out var parameter))
				{
					// This parameter is not included in the command so just continue
					continue;
				}

				var sqlParameter = (SqlParameter) parameter;
				var value = entityProperty.GetMethod.Invoke(entity, null);

				sqlParameter.SqlDbType = SqlStatement.GetSqlType(value?.GetType() ?? property.PropertyInfo.PropertyType);
				sqlParameter.Value = value ?? DBNull.Value;
			}
		}

		internal static void UpdateCommand<T>(SqlStatement statement, SqliteCommand command, T entity)
		{
			foreach (var property in statement.TableInformation.Properties)
			{
				var entityProperty = statement.TableInformation.EntityProperties[property.Name];
				var columnName = property.GetColumnName();

				if (!statement.ParametersByColumnName.TryGetValue(columnName, out var parameter))
				{
					// This parameter is not included in the command so just continue
					continue;
				}

				var value = entityProperty.GetMethod.Invoke(entity, null) ?? DBNull.Value;
				var sqlParameter = (SqliteParameter) parameter;

				if (command.Parameters.Contains(sqlParameter))
				{
					command.Parameters[sqlParameter.ParameterName].Value = value;
					continue;
				}

				sqlParameter.SqliteType = sqlParameter.SqliteType;
				sqlParameter.Value = value;

				command.Parameters.Add(sqlParameter);
			}
		}

		private static void CreateUpdateBody(SqlStatement statement, Expression expression, ref string memberName)
		{
			var isLite = statement.TableInformation.ProviderType == DatabaseProviderType.Sqlite;

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

						memberName = statement.TableInformation.PropertyToColumnName.TryGetValue(assignment.Member.Name, out var columnName) ? columnName : assignment.Member.Name;
						statement.QueryUpdate.Append($" {statement.TableInformation.ProviderPrefix}{memberName}{statement.TableInformation.ProviderSuffix}");
						statement.QueryUpdate.Append(" =");

						CreateUpdateBody(statement, assignment.Expression, ref memberName);

						if (memberInitExpression.Bindings.IndexOf(item) < memberInitExpression.Bindings.Count - 1)
						{
							statement.QueryUpdate.Append(" ,");
						}
					}
					break;
				}
				case MemberExpression memberExpression when memberExpression.Expression is ParameterExpression:
				{
					statement.QueryUpdate.Append(statement.TableInformation.PropertyToColumnName.TryGetValue(memberExpression.Member.Name, out var value)
						? isLite ? $" \"{value}\"" : $" [{value}]"
						: isLite
							? $" \"{memberExpression.Member.Name}\""
							: $" [{memberExpression.Member.Name}]");
					break;
				}
				case ConstantExpression constantExpression:
				{
					var name = statement.AddParameterValue(memberName, constantExpression.Value);
					statement.QueryUpdate.Append($" @{name}");
					memberName = null;
					break;
				}
				case UnaryExpression unaryExpression:
				{
					switch (unaryExpression.NodeType)
					{
						case ExpressionType.Convert:
							CreateUpdateBody(statement, unaryExpression.Operand, ref memberName);
							break;

						case ExpressionType.Not:
							statement.QueryUpdate.Append(" ~");
							CreateUpdateBody(statement, unaryExpression.Operand, ref memberName);
							break;
					}
					break;
				}
				case BinaryExpression binaryExpression:
				{
					var valueIsNull = binaryExpression.Right is ConstantExpression constantExpression && constantExpression.Value == null;
					CreateUpdateBody(statement, binaryExpression.Left, ref memberName);
					statement.QueryUpdate.Append(GetNodeType(binaryExpression, valueIsNull));
					if (!valueIsNull)
					{
						CreateUpdateBody(statement, binaryExpression.Right, ref memberName);
					}
					break;
				}
				default:
				{
					var value = Expression.Lambda(expression).Compile().DynamicInvoke();
					var parameterName = statement.AddParameterValue(memberName, value);
					statement.QueryUpdate.Append($" @{parameterName}");
					break;
				}
			}
		}

		private static void CreateWhere(SqlStatement statement, Expression expression)
		{
			var memberName = string.Empty;
			CreateWhere(statement, expression, ref memberName);
		}

		private static void CreateWhere(SqlStatement statement, Expression expression, ref string memberName, string propertyName = null, bool setAssignmentValue = false)
		{
			switch (expression)
			{
				case MemberExpression memberExpression:
				{
					if (setAssignmentValue)
					{
						var name = statement.AddParameterValue(memberName, memberExpression);
						statement.QueryWhere.Append($" @{name}");
						memberName = string.Empty;
					}
					else
					{
						memberName = propertyName ?? (statement.TableInformation.PropertyToColumnName.TryGetValue(memberExpression.Member.Name, out var propertyColumnName) ? propertyColumnName : memberExpression.Member.Name);

						var result = ProcessMemberExpression(memberName, statement, memberExpression);
						if (!result)
						{
							statement.QueryWhere.Append($" {statement.TableInformation.ProviderPrefix}{memberName}{statement.TableInformation.ProviderSuffix}");
						}
					}
					break;
				}
				case ConstantExpression constantExpression:
				{
					if (statement.QueryWhere.Length <= 0)
					{
						return;
					}

					var name = statement.AddParameterValue(memberName, constantExpression);
					statement.QueryWhere.Append($" @{name}");
					memberName = string.Empty;
					break;
				}
				case UnaryExpression unaryExpression:
				{
					switch (unaryExpression.NodeType)
					{
						case ExpressionType.Convert:
						{
							var body = unaryExpression.Operand.GetMemberValue("Body") as Expression;
							CreateWhere(statement, body ?? unaryExpression.Operand, ref memberName);
							break;
						}
						case ExpressionType.Quote:
						{
							var body = unaryExpression.Operand.GetMemberValue("Body") as Expression;
							CreateWhere(statement, body ?? unaryExpression.Operand, ref memberName);
							if (body != null && body.NodeType == ExpressionType.MemberAccess && body.Type == typeof(bool))
							{
								statement.QueryWhere.Append(" = 1");
							}
							memberName = string.Empty;
							break;
						}
						case ExpressionType.Not:
						{
							CreateWhere(statement, unaryExpression.Operand, ref memberName);
							if (unaryExpression.Type == typeof(bool))
							{
								statement.QueryWhere.Append(" = 0");
							}
							memberName = string.Empty;
							break;
						}
					}
					break;
				}
				case BinaryExpression binaryExpression:
				{
					CreateWhere(statement, binaryExpression.Left, ref memberName);
					if (binaryExpression.Left.NodeType == ExpressionType.MemberAccess && binaryExpression.Left.Type == typeof(bool))
					{
						statement.QueryWhere.Append(" = 1");
					}

					var valueIsNull = binaryExpression.Right is ConstantExpression { Value: null };
					statement.QueryWhere.Append(GetNodeType(binaryExpression, valueIsNull));
					if (!valueIsNull)
					{
						CreateWhere(statement, binaryExpression.Right, ref memberName, memberName, true);
					}
					break;
				}
				case MethodCallExpression methodExpression:
				{
					if (statement.QueryWhere.Length <= 0)
					{
						statement.QueryWhere.Append(" WHERE");
					}

					var offset = methodExpression.Arguments.Count == 1 ? 0 : 1;
					CreateWhere(statement, methodExpression.Arguments[offset], ref memberName);
					break;
				}
				case ParameterExpression parameterExpression:
				{
					ProcessParameterExpression(statement, parameterExpression, ref memberName);
					break;
				}
				default:
				{
					// Need to support this?
					Debug.WriteLine(expression.GetType().FullName);
					break;
				}
			}
		}

		private static void ProcessParameterExpression(SqlStatement statement, ParameterExpression expression, ref string memberName)
		{
			statement.Query.Append(memberName);
			
			if (expression.Type == typeof(bool) && expression.NodeType == ExpressionType.MemberAccess)
			{
				statement.QueryWhere.Append(" = 1");
			}
			else if (expression.Type == typeof(bool) && expression.NodeType == ExpressionType.Not)
			{
				statement.QueryWhere.Append(" = 0");
			}
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

		/// <summary>
		/// Get SQL insert script from query.
		/// </summary>
		/// <param name="statement"> The statement to process. </param>
		/// <param name="excludeTableName"> Exclude the "INTO [TableName]" from statement start </param>
		/// <returns> The SQL insert script. </returns>
		private static SqlStatement GetSqlInsert(SqlStatement statement, bool excludeTableName = false)
		{
			var columns = statement.GetSqlColumnParameterNames();
			var sqlColumnNames = statement.GetDelimitedColumnNameList(columns.Keys);
			var parameterNames = columns.Values;
			var sqlParameterNames = string.Join(", ", parameterNames.Select(x => $"@{x}"));
			statement.Query.Append(excludeTableName
				? $"INSERT ({sqlColumnNames}) VALUES ({sqlParameterNames})"
				: $"INSERT INTO {statement.TableInformation.GetFormattedTableName()} ({sqlColumnNames}) VALUES ({sqlParameterNames})"
			);
			return statement;
		}

		/// <summary>
		/// Get SQL insert script from query.
		/// </summary>
		/// <param name="statement"> The statement to process. </param>
		/// <param name="excludeTableName"> Exclude the "INTO [TableName]" from statement. </param>
		/// <param name="excludeWhere"> Exclude the WHERE clause from the statement. </param>
		/// <param name="excludedColumns"> Optional set of columns to be excluded. </param>
		/// <returns> The SQL insert script. </returns>
		private static SqlStatement GetSqlUpdate(SqlStatement statement, bool excludeTableName = false, bool excludeWhere = false, params string[] excludedColumns)
		{
			var setClause = statement.GetSetColumnList(statement.GetSqlColumnParameterNames(excludedColumns: excludedColumns));

			statement.Query.Append(excludeTableName ? "UPDATE" : $"UPDATE {statement.TableInformation.GetFormattedTableName()}");
			statement.Query.Append($" SET {setClause}");

			if (!excludeWhere)
			{
				var primaryKeys = statement.GetSqlColumnParameterNames(onlyIncludePrimaryKeys: true);
				var parameterWhere = statement.GetWhereColumnList(primaryKeys);
				statement.QueryWhere.Append(parameterWhere);
				statement.Query.Append($" WHERE {statement.QueryWhere}");
			}

			return statement;
		}

		private static bool ProcessMemberExpression(string columnName, SqlStatement statement, MemberExpression memberExpression)
		{
			if (memberExpression.Expression == null)
			{
				return false;
			}

			if (memberExpression.NodeType == ExpressionType.MemberAccess)
			{
				var propertyInfo = memberExpression.Member.GetCachedProperties().FirstOrDefault(x => x.Name == "PropertyType");
				if (propertyInfo != null)
				{
					var propertyType = (Type) propertyInfo.GetValue(memberExpression.Member);
					if (propertyType == typeof(bool))
					{
						statement.QueryWhere.Append($" {statement.TableInformation.ProviderPrefix}{columnName}{statement.TableInformation.ProviderSuffix}");
						return true;
					}
				}

				return false;
			}

			if (memberExpression.Member.DeclaringType == typeof(DateTime))
			{
				var value = memberExpression.Member.Name switch
				{
					nameof(DateTime.MinValue) => DateTime.MinValue,
					nameof(DateTime.MaxValue) => DateTime.MaxValue,
					_ => (object) null
				};

				var isLite = statement.TableInformation.ProviderType == DatabaseProviderType.Sqlite;
				if (isLite)
				{
					var name = statement.AddParameterValue(columnName, SqlDbType.DateTime2, value);
					statement.QueryWhere.Append($" @{name}");
				}
				else
				{
					var name = statement.AddParameterValue(columnName, value);
					statement.QueryWhere.Append($" @{name}");
				}
			}
			else
			{
				var value = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
				var name = statement.AddParameterValue(columnName, value);
				statement.QueryWhere.Append($" @{name}");
			}

			return true;
		}



		private static void UpdateStatementParameters<T>(SqlStatement statement, T entity)
		{
			var tableInformation = statement.TableInformation;

			tableInformation
				.EntityProperties
				.ForEach(x =>
				{
					if (!tableInformation.PropertyToColumnName.TryGetValue(x.Key, out var columnName)
						|| !statement.ParametersByColumnName.TryGetValue(columnName, out var pObject))
					{
						// Could not find the property or the parameter
						return;
					}

					// Set the parameter based on type
					switch (pObject)
					{
						case SqlParameter sqlParameter:
							sqlParameter.Value = x.Value.GetMethod.Invoke(entity, null);
							break;

						case SqliteParameter sqliteParameter:
							sqliteParameter.Value = x.Value.GetMethod.Invoke(entity, null);
							break;
					}
				});
		}

		#endregion
	}
}