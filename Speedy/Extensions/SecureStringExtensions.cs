#region References

using System;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for secure strings.
/// </summary>
public static class SecureStringExtensions
{
	#region Methods

	/// <summary>
	/// Append the provided value to the secure string.
	/// </summary>
	/// <param name="secureString"> </param>
	/// <param name="value"> </param>
	public static void Append(this SecureString secureString, SecureString value)
	{
		if (ReferenceEquals(secureString, value))
		{
			// both are the same so just return...
			return;
		}

		#if NET7_0_OR_GREATER
		var bstr = nint.Zero;
		var zero = nint.Zero;
		#else
		var bstr = IntPtr.Zero;
		var zero = IntPtr.Zero;
		#endif

		try
		{
			bstr = Marshal.SecureStringToBSTR(value);
			var length = Marshal.ReadInt32(bstr, -4);

			for (var x = 0; x < length; x += 2)
			{
				var b1 = Marshal.ReadInt16(bstr, x);
				secureString.AppendChar(Convert.ToChar(b1));
			}
		}
		finally
		{
			if (bstr != zero)
			{
				Marshal.ZeroFreeBSTR(bstr);
			}
		}
	}

	/// <summary>
	/// Compares two secure strings to see if they contain the same value.
	/// </summary>
	/// <param name="ss1"> The secure string. </param>
	/// <param name="ss2"> The matching secure string. </param>
	/// <returns> Returns true if the string match otherwise false. </returns>
	public static bool IsEqual(this SecureString ss1, SecureString ss2)
	{
		if (ReferenceEquals(ss1, ss2))
		{
			return true;
		}

		var bstr1 = IntPtr.Zero;
		var bstr2 = IntPtr.Zero;

		try
		{
			bstr1 = Marshal.SecureStringToBSTR(ss1);
			bstr2 = Marshal.SecureStringToBSTR(ss2);
			var length1 = Marshal.ReadInt32(bstr1, -4);
			var length2 = Marshal.ReadInt32(bstr2, -4);
			if (length1 == length2)
			{
				for (var x = 0; x < length1; ++x)
				{
					var b1 = Marshal.ReadByte(bstr1, x);
					var b2 = Marshal.ReadByte(bstr2, x);
					if (b1 != b2)
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}
			return true;
		}
		finally
		{
			if (bstr2 != IntPtr.Zero)
			{
				Marshal.ZeroFreeBSTR(bstr2);
			}
			if (bstr1 != IntPtr.Zero)
			{
				Marshal.ZeroFreeBSTR(bstr1);
			}
		}
	}

	/// <summary>
	/// Converts a SecureString to an unsecure string.
	/// </summary>
	/// <param name="value"> The value to be converted. </param>
	/// <returns> The unsecure string. </returns>
	public static string ToUnsecureString(this SecureString value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value));
		}

		var unmanagedString = IntPtr.Zero;

		try
		{
			unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(value);
			return Marshal.PtrToStringUni(unmanagedString);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
		}
	}

	#endregion
}