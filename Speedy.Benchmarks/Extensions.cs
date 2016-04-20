#region References

using System;
using System.Collections.ObjectModel;
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

		public static T GetRandomItem<T>(this ObservableCollection<T> collection, T exclude = null) where T : class
		{
			var random = new Random();
			var index = random.Next(0, collection.Count - 1);

			while (collection[index] == exclude)
			{
				index = random.Next(0, collection.Count - 1);
			}

			return collection[index];
		}

		#endregion
	}
}