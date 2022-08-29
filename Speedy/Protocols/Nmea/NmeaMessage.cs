#region References

using System;
using System.Collections.Generic;
using Speedy.Protocols.Nmea.Exceptions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea
{
	/// <summary>
	/// Base message
	/// </summary>
	public abstract class NmeaMessage
	{
		#region Constructors

		protected NmeaMessage(NmeaMessageType type)
		{
			Arguments = new List<string>();
			Type = type;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The checksum for this message.
		/// </summary>
		public string Checksum { get; set; }

		/// <summary>
		/// The prefix known as Talker ID.
		/// </summary>
		public NmeaMessagePrefix Prefix { get; set; }

		/// <summary>
		/// The date and time the message was received on.
		/// </summary>
		public DateTime ReceivedOn { get; set; }

		/// <summary>
		/// The sentence type of this NMEA message.
		/// </summary>
		public NmeaMessageType Type { get; }

		/// <summary>
		/// The arguments of this message.
		/// </summary>
		protected List<string> Arguments { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Take the last characters which should be the checksum
		/// </summary>
		/// <param name="sentence"> </param>
		/// <returns> </returns>
		public string ExtractChecksum(string sentence)
		{
			var index = sentence?.LastIndexOf('*') ?? -1;
			if (index == -1)
			{
				return string.Empty;
			}

			return sentence.Substring(index + 1);
		}

		/// <summary>
		/// Gets the argument or returns the default value if the index is not found.
		/// </summary>
		/// <typeparam name="T"> The type of the argument expected. </typeparam>
		/// <param name="index"> The index of the argument. </param>
		/// <param name="defaultValue"> The default value to return if not found. </param>
		/// <returns> The argument if found or default value if not. </returns>
		public T GetArgument<T>(int index, T defaultValue)
		{
			return defaultValue switch
			{
				int dValue => (T) (object) GetArgumentAsInteger(index, dValue),
				uint dValue => (T) (object) GetArgumentAsUnsignedInteger(index, dValue),
				_ => defaultValue
			};
		}

		/// <summary>
		/// Gets the argument or returns the default value if the index is not found.
		/// </summary>
		/// <param name="index"> The index of the argument. </param>
		/// <param name="defaultValue"> The default value to return if not found. </param>
		/// <returns> The argument if found or default value if not. </returns>
		public double GetArgumentAsDouble(int index, double defaultValue)
		{
			return double.TryParse(GetArgument(index), out var result) ? result : defaultValue;
		}

		/// <summary>
		/// Gets the argument or returns the default value if the index is not found.
		/// </summary>
		/// <param name="index"> The index of the argument. </param>
		/// <param name="defaultValue"> The default value to return if not found. </param>
		/// <returns> The argument if found or default value if not. </returns>
		public int GetArgumentAsInteger(int index, int defaultValue)
		{
			return int.TryParse(GetArgument(index), out var result) ? result : defaultValue;
		}

		/// <summary>
		/// Gets the argument or returns the default value if the index is not found.
		/// </summary>
		/// <param name="index"> The index of the argument. </param>
		/// <param name="defaultValue"> The default value to return if not found. </param>
		/// <returns> The argument if found or default value if not. </returns>
		public uint GetArgumentAsUnsignedInteger(int index, uint defaultValue)
		{
			return uint.TryParse(GetArgument(index), out var result) ? result : defaultValue;
		}

		public abstract void Parse(string sentence);

		/// <summary>
		/// Calculate checksum of Nmea sentence.
		/// </summary>
		/// <param name="sentence"> The Nmea sentence </param>
		/// <returns> The hexidecimal checksum </returns>
		/// <remarks>
		/// Example taken from https://gist.github.com/maxp/1193206
		/// </remarks>
		public string ParseChecksum(string sentence)
		{
			// Calculate the checksum formatted as a two-character hexadecimal
			Checksum = CalculateChecksum(sentence);
			return Checksum;
		}

		/// <summary>
		/// Reset the message to default.
		/// </summary>
		public virtual void Reset()
		{
			Arguments.Clear();
		}

		/// <summary>
		/// Update the Checksum property by using ToString() value.
		/// </summary>
		public void UpdateChecksum()
		{
			Checksum = CalculateChecksum(ToString());
		}

		/// <summary>
		/// Gets the argument for the index offset.
		/// </summary>
		/// <param name="index"> The index of the argument to cast. </param>
		/// <param name="defaultValue"> The default value if the argument index does not exists. </param>
		/// <returns> The typed argument. </returns>
		protected string GetArgument(int index, string defaultValue = "")
		{
			var response = index >= Arguments.Count ? defaultValue : Arguments[index];
			return string.IsNullOrWhiteSpace(response) ? defaultValue : response;
		}

		protected virtual void OnNmeaMessageParsed(NmeaMessage e)
		{
			NmeaMessageParsed?.Invoke(this, e);
		}

		protected void StartParse(string sentence)
		{
			Reset();

			sentence = CleanupSentence(sentence);
			var (prefix, type, value) = NmeaParser.ExtractPrefixAndType(sentence);

			if (type != Type)
			{
				throw new NmeaParseMismatchException();
			}

			Prefix = prefix;
			ParseChecksum(sentence);

			if (Checksum != ExtractChecksum(sentence))
			{
				throw new NmeaParseChecksumException($"{Checksum} != {ExtractChecksum(sentence)}");
			}

			// remove identifier plus first comma
			var values = sentence.Remove(0, value.Length);

			// remove checksum and star
			values = values.Remove(values.IndexOf('*'));

			// Assign the values as arguments
			Arguments.AddRange(values.Split(','));
		}

		internal static string CleanupSentence(string sentence)
		{
			if (sentence.EndsWith("\r") || sentence.EndsWith("\n"))
			{
				sentence = sentence.TrimEnd('\r', '\n');
			}

			var startIndex = sentence.LastIndexOf('$');

			if ((startIndex >= 0) && (startIndex != 0))
			{
				sentence = sentence.Substring(startIndex);
			}

			return startIndex == -1 ? string.Empty : sentence;
		}

		/// <summary>
		/// Generates a checksum for a NMEA sentence
		/// </summary>
		/// <param name="sentence"> The sentence to calculate for. </param>
		/// <returns> The checksum in a two-character hexadecimal format. </returns>
		private static string CalculateChecksum(string sentence)
		{
			if (string.IsNullOrWhiteSpace(sentence) || (sentence.Length < 2))
			{
				return 0.ToString("X2");
			}

			// Start with first Item
			int checksum = Convert.ToByte(sentence[sentence.IndexOf('$') + 1]);

			// Loop through all chars to get a checksum
			var start = sentence.IndexOf('$') + 2;
			var end = sentence.IndexOf('*');

			if (start > sentence.Length)
			{
				start = sentence.Length;
			}

			if (end == -1)
			{
				end = sentence.Length;
			}

			if (start > end)
			{
				start = end;
			}

			for (var i = start; i < end; i++)
			{
				// No. XOR the checksum with this character's value
				checksum ^= Convert.ToByte(sentence[i]);
			}

			// Return the checksum formatted as a two-character hexadecimal
			return checksum.ToString("X2");
		}

		#endregion

		#region Events

		public event EventHandler<NmeaMessage> NmeaMessageParsed;

		#endregion
	}
}