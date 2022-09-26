#region References

using System;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for Version
	/// </summary>
	public static class VersionExtensions
	{
		#region Methods

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

		#endregion
	}
}