#region References

using System.ComponentModel.DataAnnotations;

#endregion

namespace Speedy.Runtime;

/// <summary>
/// Represents the platform of device
/// </summary>
public enum DevicePlatform
{
	/// <summary>
	/// Unknown
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Windows
	/// </summary>
	Windows = 1,

	/// <summary>
	/// Android
	/// </summary>
	Android = 2,

	/// <summary>
	/// iOS
	/// </summary>
	[Display(Name = "iOS")]
	IOS = 3,

	/// <summary>
	/// Mac OS
	/// </summary>
	MacOS = 4,

	/// <summary>
	/// Linux
	/// </summary>
	Linux = 5
}