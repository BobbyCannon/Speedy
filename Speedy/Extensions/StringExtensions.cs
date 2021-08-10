#region References

using System;
using System.Globalization;
using System.Text;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the string type.
	/// </summary>
	public static class StringExtensions
	{
		#region Methods

		/// <summary>
		/// Convert a hex string to a byte array.
		/// </summary>
		/// <param name="hexString"> A string with hex data (2 bytes per character). </param>
		/// <returns> The byte array value of the hex string. </returns>
		public static byte[] ConvertHexStringToByteArray(this string hexString)
		{
			if (hexString.Length % 2 != 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
			}

			var data = new byte[hexString.Length / 2];
			for (var index = 0; index < data.Length; index++)
			{
				var byteValue = hexString.Substring(index * 2, 2);
				data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}

			return data;
		}

		/// <summary>
		/// Convert a HEX string to a regular text string.
		/// </summary>
		/// <param name="value"> A string with hex data (2 bytes per character). </param>
		/// <returns> The string value. </returns>
		public static string FromHexString(this string value)
		{
			var bytes = ConvertHexStringToByteArray(value);
			var response = Encoding.UTF8.GetString(bytes);
			return response;
		}

		/// <summary>
		/// Convert the hex string back to byte array.
		/// </summary>
		/// <param name="value"> The hex string to be converter. </param>
		/// <returns> The byte array. </returns>
		public static byte[] FromHexStringToArray(this string value)
		{
			var bytes = new byte[value.Length / 2];

			for (var i = 0; i < bytes.Length; i++)
			{
				bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
			}

			return bytes;
		}

		/// <summary>
		/// Trims string to a maximum length.
		/// </summary>
		/// <param name="value"> The value to process. </param>
		/// <param name="max"> The maximum length of the string. </param>
		/// <param name="addEllipses"> The option to add ellipses to shorted strings. Defaults to false. </param>
		/// <returns> The value limited to the maximum length. </returns>
		public static string MaxLength(this string value, int max, bool addEllipses = false)
		{
			if (string.IsNullOrWhiteSpace(value) || max <= 0)
			{
				return string.Empty;
			}

			var shouldAddEllipses = addEllipses && value.Length > max && max >= 4;

			return value.Length > max ? value.Substring(0, shouldAddEllipses ? max - 3 : max) + (shouldAddEllipses ? "..." : string.Empty) : value;
		}

		/// <summary>
		/// Converts a string to hex string value. Ex. "A" -> "41"
		/// </summary>
		/// <param name="value"> The string value to convert. </param>
		/// <returns> The string in a hex string format. </returns>
		public static string ToHexString(this string value)
		{
			var bytes = Encoding.Default.GetBytes(value);
			var hexString = bytes.ToHexString();
			return hexString;
		}

		/// <summary>
		/// Converts a byte array to a hex string format. Ex. [41],[42] = "4142"
		/// </summary>
		/// <param name="data"> The byte array to convert. </param>
		/// <returns> The byte array in a hex string format. </returns>
		public static string ToHexString(this Guid data)
		{
			var hexString = BitConverter.ToString(data.ToByteArray());
			hexString = hexString.Replace("-", "");
			return hexString;
		}

		/// <summary>
		/// Converts a byte array to a hex string format. Ex. [41],[42] = "4142"
		/// </summary>
		/// <param name="data"> The byte array to convert. </param>
		/// <returns> The byte array in a hex string format. </returns>
		public static string ToHexString(this byte[] data)
		{
			var hexString = BitConverter.ToString(data);
			hexString = hexString.Replace("-", "");
			return hexString;
		}

		/// <summary>
		/// To literal version of the string.
		/// </summary>
		/// <param name="input"> The string input. </param>
		/// <returns> The literal version of the string. </returns>
		public static string ToLiteral(this string input)
		{
			if (input == null)
			{
				return "null";
			}

			var literal = new StringBuilder(input.Length);
			literal.Append("\"");
			foreach (var c in input)
			{
				switch (c)
				{
					case '\'':
						literal.Append(@"\'");
						break;
					case '\"':
						literal.Append("\\\"");
						break;
					case '\\':
						literal.Append(@"\\");
						break;
					case '\0':
						literal.Append(@"\0");
						break;
					case '\a':
						literal.Append(@"\a");
						break;
					case '\b':
						literal.Append(@"\b");
						break;
					case '\f':
						literal.Append(@"\f");
						break;
					case '\n':
						literal.Append(@"\n");
						break;
					case '\r':
						literal.Append(@"\r");
						break;
					case '\t':
						literal.Append(@"\t");
						break;
					case '\v':
						literal.Append(@"\v");
						break;
					default:
						// ASCII printable character
						if (c >= 0x20 && c <= 0x7e)
						{
							literal.Append(c);
							// As UTF16 escaped character
						}
						else
						{
							literal.Append(@"\u");
							literal.Append(((int) c).ToString("x4"));
						}

						break;
				}
			}

			literal.Append("\"");
			return literal.ToString();
		}

		#endregion
	}
}