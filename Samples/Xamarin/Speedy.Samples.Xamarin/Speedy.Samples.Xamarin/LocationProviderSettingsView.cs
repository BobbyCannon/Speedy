#region References

using System;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Samples.Xamarin
{
	public class LocationProviderSettingsView : LocationProviderSettings
	{
		#region Properties

		/// <summary>
		/// Cannot bind to static properties, so we must create a "view" version.
		/// </summary>
		public double DesiredAccuracyLowerLimitView => DesiredAccuracyLowerLimit;

		/// <summary>
		/// Cannot bind to static properties, so we must create a "view" version.
		/// </summary>
		public double DesiredAccuracyUpperLimitView => DesiredAccuracyUpperLimit;

		/// <summary>
		/// Cannot bind to static properties, so we must create a "view" version.
		/// </summary>
		public double MinimumDistanceLowerLimitView => MinimumDistanceLowerLimit;

		/// <summary>
		/// Cannot bind to static properties, so we must create a "view" version.
		/// </summary>
		public double MinimumDistanceUpperLimitView => MinimumDistanceUpperLimit;

		public int MinimumTimeInSeconds
		{
			get => (int) MinimumTime.TotalSeconds;
			set => MinimumTime = TimeSpan.FromSeconds(value);
		}

		public int MinimumTimeLowerLimitInSeconds
		{
			get => (int) MinimumTimeLowerLimit.TotalSeconds;
			set => MinimumTimeLowerLimit = TimeSpan.FromSeconds(value);
		}

		public int MinimumTimeUpperLimitInSeconds
		{
			get => (int) MinimumTimeUpperLimit.TotalSeconds;
			set => MinimumTimeUpperLimit = TimeSpan.FromSeconds(value);
		}

		#endregion

		#region Methods

		protected override void OnPropertyChangedInDispatcher(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(DesiredAccuracyLowerLimit):
				{
					OnPropertyChanged(nameof(DesiredAccuracyLowerLimitView));
					break;
				}
				case nameof(DesiredAccuracyUpperLimit):
				{
					OnPropertyChanged(nameof(DesiredAccuracyUpperLimitView));
					break;
				}
				case nameof(MinimumDistanceLowerLimit):
				{
					OnPropertyChanged(nameof(MinimumDistanceLowerLimitView));
					break;
				}
				case nameof(MinimumDistanceUpperLimit):
				{
					OnPropertyChanged(nameof(MinimumDistanceUpperLimitView));
					break;
				}
				case nameof(MinimumTime):
				{
					OnPropertyChanged(nameof(MinimumTimeInSeconds));
					break;
				}
				case nameof(MinimumTimeLowerLimit):
				{
					OnPropertyChanged(nameof(MinimumTimeLowerLimitInSeconds));
					break;
				}
				case nameof(MinimumTimeUpperLimit):
				{
					OnPropertyChanged(nameof(MinimumTimeUpperLimitInSeconds));
					break;
				}
			}

			base.OnPropertyChangedInDispatcher(propertyName);
		}

		#endregion
	}
}