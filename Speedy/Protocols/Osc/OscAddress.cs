#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscAddress : IEnumerable<string>
	{
		#region Fields

		private List<string> _addressParts;
		private static readonly Regex _literalAddressValidator;

		#endregion

		#region Constructors

		public OscAddress(string address)
		{
			_addressParts = new List<string>();

			ParseAddress(address);
		}

		static OscAddress()
		{
			_literalAddressValidator = new Regex(@"^/[^\s#\*,/\?\[\]\{}]+((/[^\s#\*,/\?\[\]\{}]+)*)$", RegexOptions.Compiled);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Access each index of the address parts.
		/// </summary>
		/// <param name="index"> The index of the part. </param>
		/// <returns> The address part at the given index. </returns>
		public string this[int index] => _addressParts[index];

		/// <summary>
		/// The original string value of the address.
		/// </summary>
		public string Value { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public IEnumerator<string> GetEnumerator()
		{
			return _addressParts.GetEnumerator();
		}

		/// <summary>
		/// Is the supplied address a valid literal address (no wildcards or lists)
		/// </summary>
		/// <param name="address"> the address to check </param>
		/// <returns> true if the address is valid </returns>
		public static bool IsValidAddress(string address)
		{
			return !string.IsNullOrWhiteSpace(address) && _literalAddressValidator.IsMatch(address);
		}

		/// <summary>
		/// Sees if the address matches the provide address
		/// </summary>
		/// <param name="address"> The address to check for match. </param>
		/// <returns> Return true if the address matches the provided address. </returns>
		public bool Matches(string address)
		{
			// Only have literal match right now.
			return Value.Equals(address);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Parse the OSC address from a string.
		/// </summary>
		/// <param name="address"> The string value of the OSC address. </param>
		private void ParseAddress(string address)
		{
			// Ensure address is valid
			if (!IsValidAddress(address))
			{
				throw new ArgumentException($"The address '{address}' is not a valid osc address", nameof(address));
			}

			// Break down the parts
			_addressParts = address.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

			// Keep the original value
			Value = address;
		}

		#endregion
	}
}