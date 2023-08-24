#region References

using System.ComponentModel.DataAnnotations;

#endregion

namespace Speedy.Data;

/// <summary>
/// Represents the bitness of a platform, application, etc
/// </summary>
public enum Bitness
{
	/// <summary>
	/// Unknown
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Represents x86 or 32 bitness.
	/// </summary>
	[Display(Name = "32 bit", ShortName = "x86")]
	X86 = 1,

	/// <summary>
	/// Represents x64 or 64 bitness.
	/// </summary>
	[Display(Name = "64 bit", ShortName = "x64")]
	X64 = 2
}