#region References

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for assembly.
/// </summary>
public static class AssemblyExtensions
{
	#region Methods

	/// <summary>
	/// Gets the directory the assembly file exists in.
	/// </summary>
	/// <param name="assembly"> The assembly to be tested. </param>
	/// <returns> The directory info for the assembly. </returns>
	public static DirectoryInfo GetAssemblyDirectory(this Assembly assembly)
	{
		var response =
			#if (NET7_0_OR_GREATER)
				Path.GetDirectoryName(assembly.Location)
			#else
			Path.GetDirectoryName(assembly.CodeBase)
			#endif
			?? AppDomain.CurrentDomain.BaseDirectory;

		if (response.StartsWith("file:\\"))
		{
			response = response.Substring(6);
		}

		return new DirectoryInfo(response);
	}

	/// <summary>
	/// Returns true if the assembly is a debug build.
	/// </summary>
	/// <param name="assembly"> The assembly to check. </param>
	/// <returns> True if the assembly is a debug build otherwise false. </returns>
	public static bool IsAssemblyDebugBuild(this Assembly assembly)
	{
		return assembly.GetCustomAttributes(false)
			.OfType<DebuggableAttribute>()
			.Any(x => x.IsJITTrackingEnabled
				|| x.IsJITOptimizerDisabled
				|| x.DebuggingFlags.HasFlag(DebuggableAttribute.DebuggingModes.DisableOptimizations)
			);
	}

	#endregion
}