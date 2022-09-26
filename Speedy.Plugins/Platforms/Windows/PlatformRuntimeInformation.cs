﻿#region References

using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.System.Profile;
using Speedy.Plugins.Maui;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui.Devices
{
	public class PlatformRuntimeInformation : MauiRuntimeInformation
	{
		#region Properties

		public override string DeviceId
		{
			get
			{
				var systemId = SystemIdentification.GetSystemIdForPublisher();
				if (systemId == null)
				{
					return null;
				}

				var systemIdBytes = systemId.Id.ToArray();
				var encoder = new Base32ByteArrayEncoder(Base32ByteArrayEncoder.CrockfordAlphabet);
				return encoder.Encode(systemIdBytes);
			}
		}

		#endregion

		#region Classes

		/// <summary>
		/// An implementation of encodes byte arrays as Base32 strings.
		/// </summary>
		private class Base32ByteArrayEncoder
		{
			#region Fields

			/// <summary>
			/// Gets the alphabet in use.
			/// </summary>
			private readonly string _alphabet;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="Base32ByteArrayEncoder" /> class.
			/// </summary>
			/// <param name="alphabet"> The alphabet to use. </param>
			public Base32ByteArrayEncoder(string alphabet)
			{
				if (alphabet is null)
				{
					throw new ArgumentNullException(nameof(alphabet));
				}

				if (alphabet.Length != 32)
				{
					throw new ArgumentException("The alphabet must have a length of 32.", nameof(alphabet));
				}

				_alphabet = alphabet;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets the Crockford Base32 alphabet.
			/// </summary>
			/// <remarks>
			/// See https://www.crockford.com/base32.html
			/// </remarks>
			public static string CrockfordAlphabet { get; } = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

			/// <summary>
			/// Gets the RFC-4648 Base32 alphabet.
			/// </summary>
			/// <remarks>
			/// See https://datatracker.ietf.org/doc/html/rfc4648#section-6
			/// </remarks>
			public static string Rfc4648Alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

			#endregion

			#region Methods

			/// <summary>
			/// Encodes the specified byte array as a string.
			/// </summary>
			/// <param name="bytes"> The byte array to encode. </param>
			/// <returns> The byte array encoded as a string. </returns>
			public string Encode(byte[] bytes)
			{
				if (bytes == null)
				{
					throw new ArgumentNullException(nameof(bytes));
				}

				if (bytes.Length == 0)
				{
					return string.Empty;
				}

				const int shift = 5;
				const int mask = 31;

				var outputLength = (((bytes.Length * 8) + shift) - 1) / shift;
				var sb = new StringBuilder(outputLength);

				var offset = 0;
				var last = bytes.Length;
				int buffer = bytes[offset++];
				var bitsLeft = 8;
				while ((bitsLeft > 0) || (offset < last))
				{
					if (bitsLeft < shift)
					{
						if (offset < last)
						{
							buffer <<= 8;
							buffer |= bytes[offset++] & 0xff;
							bitsLeft += 8;
						}
						else
						{
							var pad = shift - bitsLeft;
							buffer <<= pad;
							bitsLeft += pad;
						}
					}

					var index = mask & (buffer >> (bitsLeft - shift));
					bitsLeft -= shift;
					sb.Append(_alphabet[index]);
				}

				return sb.ToString();
			}

			#endregion
		}

		#endregion
	}
}