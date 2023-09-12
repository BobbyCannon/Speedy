#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// Represents a provider of data. This is the base contract that only provider the ID for the provider.
/// </summary>
public interface IProvider
{
	#region Methods

	/// <summary>
	/// Gets the ID of the provider.
	/// </summary>
	/// <returns> </returns>
	public Guid GetProviderId();

	#endregion
}