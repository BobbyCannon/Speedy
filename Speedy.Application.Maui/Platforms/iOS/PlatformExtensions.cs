#region References

using Foundation;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui;

/// <summary>
/// iOS Platform extensions
/// </summary>
public static class PlatformExtensions
{
	#region Methods

	/// <summary>
	/// Converts NSDate to DateTime.
	/// </summary>
	public static DateTime ToDateTime(this NSDate date)
	{
		return (DateTime) date;
	}

	#endregion
}