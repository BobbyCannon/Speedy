#region References

using System;
using Microsoft.Data.Sqlite;

#endregion

namespace Speedy.DataInformation
{
	public class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			ShowSqliteDataTypes(byte.MinValue,
				short.MinValue, ushort.MinValue,
				int.MinValue, uint.MinValue,
				long.MinValue, ulong.MinValue,
				DateTime.MinValue, "string.Empty",
				(DateTime?) DateTime.MaxValue,
				double.MinValue, float.MinValue);
		}

		private static void ShowSqliteDataTypes(params object[] values)
		{
			Console.WriteLine("Sqlite Data Types");

			foreach (var value in values)
			{
				var parameter = new SqliteParameter("p", value);
				Console.WriteLine($"\t{value.GetType().FullName} -> {parameter.SqliteType}");
			}
		}

		#endregion
	}
}