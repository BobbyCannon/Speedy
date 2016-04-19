#region References

using Speed.Benchmarks.Properties;
using Speedy.EntityFramework;

#endregion

namespace Speed.Benchmarks
{
	public static class Extensions
	{
		#region Methods

		public static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
		{
			database.Database.ExecuteSqlCommand(Resources.ClearDatabase);
			return database;
		}

		#endregion
	}
}