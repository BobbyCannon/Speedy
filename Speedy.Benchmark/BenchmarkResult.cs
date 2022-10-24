#region References

using System;
using System.Diagnostics;

#endregion

namespace Speedy.Benchmark
{
	public class BenchmarkResult : Bindable
	{
		#region Fields

		private readonly Stopwatch _watch;

		#endregion

		#region Constructors

		public BenchmarkResult()
		{
			_watch = Stopwatch.StartNew();
		}

		#endregion

		#region Properties

		public TimeSpan Elapsed => _watch.Elapsed;

		public string Name { get; set; }

		public double Percent { get; set; }

		#endregion

		#region Methods

		protected override void PropertyHasChanged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(Percent):
				{
					OnPropertyChanged(nameof(Elapsed));
					break;
				}
			}

			base.PropertyHasChanged(propertyName);
		}

		public void Stop()
		{
			_watch.Stop();
			OnPropertyChanged(nameof(Elapsed));
		}

		#endregion
	}
}