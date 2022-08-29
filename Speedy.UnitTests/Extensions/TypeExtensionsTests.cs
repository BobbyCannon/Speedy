#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class TypeExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void IsNullable()
		{
			var nullableTypes = new[]
			{
				typeof(string),
				typeof(DBNull)
			};

			foreach (var type in nullableTypes)
			{
				Assert.IsTrue(type.IsNullable());
			}

			var nonNullableTypes = new[]
			{
				typeof(bool),
				typeof(ConsoleKey),
				typeof(DateTime),
				typeof(int),
				typeof(double),
				typeof(SyncObject)
			};

			foreach (var type in nonNullableTypes)
			{
				Assert.IsFalse(type.IsNullable());
			}
		}

		#endregion
	}
}