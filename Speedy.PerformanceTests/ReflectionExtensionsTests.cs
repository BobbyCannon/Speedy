#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Client;
using Speedy.Extensions;

#endregion

namespace Speedy.PerformanceTests
{
	[TestClass]
	public class ReflectionExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void LoopForNew()
		{
			var collection = new List<object>();
			var type = typeof(ClientAccount);
			// warmup?
			ReflectionExtensions.CreateInstance(type);
			var watch = Stopwatch.StartNew();

			for (var i = 0; i < 1000; i++)
			{
				var item = ReflectionExtensions.CreateInstance(type);
				collection.Add(item);
			}

			watch.Stop();

			Console.WriteLine(watch.Elapsed);

			watch.Restart();

			for (var i = 0; i < 1000; i++)
			{
				var item = Activator.CreateInstance(type);
				collection.Add(item);
			}

			watch.Stop();

			Console.WriteLine(watch.Elapsed);
		}

		#endregion
	}
}