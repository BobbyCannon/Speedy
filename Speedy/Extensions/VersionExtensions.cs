#region References

using System;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for Version
/// </summary>
public static class VersionExtensions
{
	#region Methods

	/// <summary>
	/// Converts a version hash to a version number.
	/// </summary>
	/// <param name="version"> The version hash as a decimal. </param>
	/// <returns> The version number. </returns>
	public static Version FromVersionHash(this ulong version)
	{
		//                           12345123451234512345
		var data = version.ToString("00000000000000000000");
		var revision = int.Parse(data.Substring(data.Length - 5, 5));
		var build = int.Parse(data.Substring(data.Length - 10, 5));
		var minor = int.Parse(data.Substring(data.Length - 15, 5));
		var major = int.Parse(data.Substring(0, data.Length - 15));
		return new Version(major, minor, build, revision);
	}

	/// <summary>
	/// Checks to see if version is default (not set). Ex. 0.0.0.0
	/// </summary>
	/// <param name="version"> The version to check. </param>
	/// <returns> True if the version is the default value of 0.0.0.0. </returns>
	public static bool IsDefault(this Version version)
	{
		return (version.Major == 0)
			&& (version.Minor == 0)
			&& version.Build is -1 or 0
			&& version.Revision is -1 or 0;
	}

	/// <summary>
	/// Converts a version number into a version hash.
	/// </summary>
	/// <param name="version"> The version to be converted. </param>
	/// <returns> The hash of the version. </returns>
	public static ulong ToVersionHash(this Version version)
	{
		// 18446744073709551615
		// 12345123451234512345

		var build = version.Build >= 0 ? version.Build.ToString("D5") : "00000";
		var revision = version.Revision >= 0 ? version.Revision.ToString("D5") : "00000";
		return ulong.Parse(version.Major + version.Minor.ToString("D5") + build + revision);
	}

	#endregion
}