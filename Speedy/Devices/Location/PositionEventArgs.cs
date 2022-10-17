using System;

namespace Speedy.Devices.Location;

/// <summary>
/// ProviderLocation args
/// </summary>
public class PositionEventArgs : EventArgs
{
	#region Constructors

	/// <summary>
	/// ProviderLocation args
	/// </summary>
	/// <param name="position"> </param>
	public PositionEventArgs(IProviderLocation position)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}

		ProviderLocation = position;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The ProviderLocation
	/// </summary>
	public IProviderLocation ProviderLocation { get; }

	#endregion
}