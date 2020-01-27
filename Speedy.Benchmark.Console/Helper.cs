#region References

using System;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;

#endregion

namespace Speedy.Benchmark
{
	public static class Helper
	{
		#region Methods

		public static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
		{
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			//var command = "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; IF ''?'' NOT LIKE ''%MigrationHistory%'' AND ''?'' NOT LIKE ''%MigrationsHistory%'' DELETE FROM ?'\r\nEXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)'";
			//database.Database.ExecuteSqlCommand(command);
			return database;
		}

		public static void Dump<T>(this T item, string prefix = "")
		{
			Console.Write(prefix);
			Console.WriteLine(item);
		}

		#endregion
	}
}