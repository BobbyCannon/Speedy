#region References

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

#pragma warning disable 1591

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscStatisticValue : INotifyPropertyChanged
	{
		#region Fields

		private long _lastTotal;
		private DateTime _lastUpdate;

		#endregion

		#region Constructors

		public OscStatisticValue(string name)
		{
			Name = name;
			UpdateInterval = TimeSpan.FromMilliseconds(1000);
		}

		#endregion

		#region Properties

		public string Name { get; }

		public float Rate { get; private set; }

		public long Total { get; private set; }

		public TimeSpan UpdateInterval { get; set; }

		#endregion

		#region Methods

		public void Increment(int amount = 1)
		{
			Total += amount;
		}

		public void InvokePropertyChanged()
		{
			OnPropertyChanged(nameof(Total));
			OnPropertyChanged(nameof(Rate));
		}

		public void Reset()
		{
			Rate = 0;
			Total = 0;

			_lastTotal = 0;
		}

		public void UpdateRate()
		{
			var now = TimeService.Now;
			var time = now - _lastUpdate;

			if (time < UpdateInterval)
			{
				return;
			}

			var diff = Total - _lastTotal;
			Rate = diff / (float) time.TotalSeconds;

			_lastTotal = Total;
			_lastUpdate = now;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}