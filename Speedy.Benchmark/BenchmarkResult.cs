#region References

using System;

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

		public override void OnPropertyChanged(string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Percent):
					OnPropertyChanged(nameof(Elapsed));
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

		public void Stop()
		{
			_watch.Stop();
			OnPropertyChanged(nameof(Elapsed));
		}

		#endregion
	}
}