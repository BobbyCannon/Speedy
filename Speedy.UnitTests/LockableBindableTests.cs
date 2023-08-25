#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;

#endregion

namespace Speedy.UnitTests;

[TestClass]
public class LockableBindableTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void ShareReads()
	{
		var test = new LockableBindable();
		var options = new ParallelOptions { MaxDegreeOfParallelism = 32 };
		var list = new List<int> { 0, 1, 2, 3, 4, 5 };
		var watch = Stopwatch.StartNew();
		var threadStarts = new List<TimeSpan>();

		Parallel.For(0, list.Count, options, i =>
		{
			test.ReadLock(() =>
			{
				threadStarts.Add(watch.Elapsed);
				list[i].Dump(watch.Elapsed.ToString());
				Thread.Sleep(10);
			});
		});

		watch.Stop();
		watch.Elapsed.Dump();

		foreach (var start in threadStarts)
		{
			IsTrue(start.TotalMilliseconds < 10);
		}
	}

	[TestMethod]
	public void WriteShouldInterrupt()
	{
		var test = new LockableBindable();
		var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
		var list = new int[options.MaxDegreeOfParallelism * 2];
		for (var i = 0; i < list.Length; i++)
		{
			list[i] = i;
		}
		var watch = Stopwatch.StartNew();

		Parallel.For(0, list.Length, options, i =>
		{
			if ((RandomGenerator.NextInteger(1, 10) % 3) == 0)
			{
				test.UpgradeableReadLock(() =>
				{
					Thread.Sleep(5);

					test.WriteLock(() =>
					{
						list[i].Dump($"{i} - U {watch.Elapsed}");
						Thread.Sleep(10);
					});
				});

				return;
			}

			if ((i % 3) == 0)
			{
				test.WriteLock(() =>
				{
					list[i].Dump($"{i} - W {watch.Elapsed}");
					Thread.Sleep(10);
				});

				return;
			}

			test.ReadLock(() =>
			{
				list[i].Dump($"{i} - R {watch.Elapsed}");
				Thread.Sleep(0);
			});
		});
	}

	#endregion
}