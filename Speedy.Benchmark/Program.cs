#region References

using System;

#endregion

namespace Speedy.Benchmark
{
	public static class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			//SyncEntityBenchmark.Run();
			SyncClientBenchmark.Run();

			Console.WriteLine("press any key");
			Console.ReadKey(true);
		}

		#endregion
	}
}