#region References

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
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
		#region Fields

		private static readonly PropertyInfo _databaseDependenciesField;
		private static readonly FieldInfo _dataBaseField;
		private static readonly FieldInfo _queryCompilerField;
		private static readonly FieldInfo _queryModelGeneratorField;

		#endregion

		#region Constructors

		static SqlExtensions()
		{
			var queryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

			_databaseDependenciesField = typeof(EFDatabase).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");
			_dataBaseField = queryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
			_queryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
			_queryModelGeneratorField = queryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");
		}

		#endregion

		#region Methods

		/// <summary>
		/// Convert the Queryable to a SQL query.
		/// </summary>
		/// <typeparam name="TEntity"> The entity type. </typeparam>
		/// <param name="query"> The query for the entity. </param>
		/// <returns> The SQL query for the queryable. </returns>
		public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
		{
			var queryCompiler = (QueryCompiler) _queryCompilerField.GetValue(query.Provider);
			var modelGenerator = (QueryModelGenerator) _queryModelGeneratorField.GetValue(queryCompiler);
			var queryModel = modelGenerator.ParseQuery(query.Expression);
			var database = (IEFDatabase) _dataBaseField.GetValue(queryCompiler);
			var databaseDependencies = (DatabaseDependencies) _databaseDependenciesField.GetValue(database);
			var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
			var modelVisitor = (RelationalQueryModelVisitor) queryCompilationContext.CreateQueryModelVisitor();
			
			modelVisitor.CreateQueryExecutor<TEntity>(queryModel);

			return modelVisitor.Queries.First().ToString();
		}

		#endregion
	}
}