#region References

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Speedy.Extensions;
using EFDatabase = Microsoft.EntityFrameworkCore.Storage.Database;
using IEFDatabase = Microsoft.EntityFrameworkCore.Storage.IDatabase;

#endregion

namespace Speedy.EntityFramework.Sql
{
	/// <summary>
	/// Extension to support SQL only functionality
	/// </summary>
	public static class SqlExtensions
	{
		#region Methods

		/// <summary>
		/// Convert the Queryable to a SQL query.
		/// </summary>
		/// <typeparam name="TEntity"> The entity type. </typeparam>
		/// <param name="query"> The query for the entity. </param>
		/// <returns> The SQL query for the queryable. </returns>
		public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
		{
			var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
			var relationalCommandCache = enumerator.Private("_relationalCommandCache");
			var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
			var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");
			var sqlGenerator = factory.Create();
			var command = sqlGenerator.GetCommand(selectExpression);
			return command.CommandText;
		}

		private static object Private(this object obj, string privateField)
		{
			return obj?.GetType().GetCachedField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
		}

		private static T Private<T>(this object obj, string privateField)
		{
			return (T) obj?.GetType().GetCachedField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
		}

		#endregion
	}
}