#region References

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using Microsoft.Win32.SafeHandles;

#endregion

namespace Speedy.Application.Wpf.Security
{
	public static class WindowsCredentialManager
	{
		#region Methods

		public static void Delete(WindowsCredential credential)
		{
			CredDelete(credential.ApplicationName, credential.CredentialType, 0);
			//Trace.Assert(result, "Failed to delete the credential.");
		}

		public static IEnumerable<WindowsCredential> EnumerateCredentials(Func<WindowsCredential, bool> filter = null)
		{
			var pCredentials = IntPtr.Zero;
			var response = new List<WindowsCredential>();

			try
			{
				var result = CredEnumerate(null, 0, out var count, out pCredentials);
				if (result)
				{
					for (var i = 0; i < count; i++)
					{
						var credential = Marshal.ReadIntPtr(pCredentials, i * Marshal.SizeOf(typeof(IntPtr)));
						using (var handle = new CriticalCredentialHandle(credential, true))
						{
							var c = ConvertCredential(handle.GetCredential());
							if ((filter != null) && !filter(c))
							{
								continue;
							}

							response.Add(c);
						}
					}
				}
			}
			finally
			{
				CredFree(pCredentials);
			}

			return response;
		}

		public static WindowsCredential ReadCredential(string applicationName)
		{
			var read = CredRead(applicationName, CredentialType.Generic, 0, out var nCredPtr);
			if (!read)
			{
				return null;
			}

			using var handle = new CriticalCredentialHandle(nCredPtr, false);
			return ConvertCredential(handle.GetCredential());
		}

		public static WindowsCredential WriteCredential(string applicationName, string userName, SecureString password)
		{
			return WriteCredential(new WindowsCredential(CredentialType.Generic, applicationName, userName, password));
		}

		public static WindowsCredential WriteCredential(WindowsCredential credential)
		{
			if (credential.Password.Length > 256)
			{
				throw new ArgumentOutOfRangeException(nameof(credential), "The password has exceeded 256 bytes.");
			}

			var blob = Marshal.SecureStringToBSTR(credential.Password);
			var nativeCredential = new NativeCredential
			{
				AttributeCount = 0,
				Attributes = IntPtr.Zero,
				Comment = credential.Comment == null ? IntPtr.Zero : Marshal.StringToCoTaskMemUni(credential.Comment),
				TargetAlias = IntPtr.Zero,
				Type = CredentialType.Generic,
				Persist = (uint) CredentialPersistence.LocalMachine,
				CredentialBlobSize = (uint) credential.Password.Length * 2,
				TargetName = Marshal.StringToCoTaskMemUni(credential.ApplicationName),
				CredentialBlob = blob,
				UserName = Marshal.StringToCoTaskMemUni(credential.UserName ?? Environment.UserName)
			};

			var written = CredWrite(ref nativeCredential, 0);
			var lastError = Marshal.GetLastWin32Error();
			var response = ConvertCredential(nativeCredential);

			Marshal.FreeCoTaskMem(nativeCredential.Comment);
			Marshal.FreeCoTaskMem(nativeCredential.TargetName);
			Marshal.ZeroFreeBSTR(nativeCredential.CredentialBlob);
			Marshal.FreeCoTaskMem(nativeCredential.UserName);

			if (!written)
			{
				throw new Exception($"CredWrite failed with the error code {lastError}.");
			}

			return response;
		}

		private static WindowsCredential ConvertCredential(NativeCredential nativeCredential)
		{
			var applicationName = Marshal.PtrToStringUni(nativeCredential.TargetName);
			var userName = Marshal.PtrToStringUni(nativeCredential.UserName);
			var comment = Marshal.PtrToStringUni(nativeCredential.Comment);
			var secret = new SecureString();

			if (nativeCredential.CredentialBlob != IntPtr.Zero)
			{
				for (var i = 0; i < nativeCredential.CredentialBlobSize; i += 2)
				{
					secret.AppendChar((char) Marshal.ReadInt16(nativeCredential.CredentialBlob, i));
				}
			}

			var credential = new WindowsCredential(nativeCredential.Type, applicationName, userName, secret, comment);
			return credential;
		}

		[DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
		private static extern bool CredDelete(string target, CredentialType type, int flags);

		[DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool CredEnumerate(string filter, int flags, out int count, out IntPtr pCredentials);

		[DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
		private static extern void CredFree([In] IntPtr cred);

		[DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credentialPtr);

		[DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool CredWrite([In] ref NativeCredential userNativeCredential, [In] uint flags);

		#endregion

		#region Classes

		private sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
		{
			#region Fields

			private readonly bool _isPartOfEnumeration;

			#endregion

			#region Constructors

			public CriticalCredentialHandle(IntPtr preexistingHandle, bool isPartOfEnumeration)
			{
				_isPartOfEnumeration = isPartOfEnumeration;
				SetHandle(preexistingHandle);
			}

			#endregion

			#region Methods

			public NativeCredential GetCredential()
			{
				if (IsInvalid)
				{
					throw new InvalidOperationException("Invalid Critical Handle!");
				}

				return (NativeCredential) Marshal.PtrToStructure(handle, typeof(NativeCredential));
			}

			protected override bool ReleaseHandle()
			{
				if (IsInvalid)
				{
					return false;
				}

				if (!_isPartOfEnumeration)
				{
					// Only free credentials when doing single read.
					CredFree(handle);
				}

				SetHandleAsInvalid();
				return true;
			}

			#endregion
		}

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct NativeCredential
		{
			public readonly uint Flags;
			public CredentialType Type;
			public IntPtr TargetName;
			public IntPtr Comment;
			public readonly FILETIME LastWritten;
			public uint CredentialBlobSize;
			public IntPtr CredentialBlob;
			public uint Persist;
			public uint AttributeCount;
			public IntPtr Attributes;
			public IntPtr TargetAlias;
			public IntPtr UserName;
		}

		#endregion

		#region Enumerations

		public enum CredentialType
		{
			Generic = 1,
			DomainPassword = 2,
			DomainCertificate = 3,
			DomainVisiblePassword = 4,
			GenericCertificate = 5,
			DomainExtended = 6,
			Maximum = 7,
			MaximumEx = Maximum + 1000
		}

		private enum CredentialPersistence : uint
		{
			Session = 1,
			LocalMachine = 2,
			Enterprise = 3
		}

		#endregion
	}
}