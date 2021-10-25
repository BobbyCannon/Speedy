#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages
{
	/// <summary>
	/// Represents a TXT message.
	/// </summary>
	public class TxtMessage : NmeaMessage
	{
		#region Constructors

		public TxtMessage() : base(NmeaMessageType.TXT)
		{
		}

		#endregion

		#region Properties

		public int SentenceNumber { get; set; }

		public string Text { get; set; }

		public string TextIdentifier { get; set; }

		public int TotalNumberOfSentences { get; set; }

		#endregion

		#region Methods

		public override void Parse(string sentence)
		{
			// $GPTXT,01,01,25,DR MODE - ANTENNA FAULT^21*38
			//
			// .      0  1  2  3   4
			//        |  |  |  |   |
			// $--TXT,xx,xx,xx,c-c*hh
			//
			// 0) Total Number of Sentences 01 to 99
			// 1) Sentence Number 01 to 99
			// 2) Text Identifier
			// 3) Text Message
			// 4) Checksum

			StartParse(sentence);

			TotalNumberOfSentences = int.TryParse(GetArgument(0, "0"), out var t) ? t : 0;
			SentenceNumber = int.TryParse(GetArgument(1, "0"), out var n) ? n : 0;
			TextIdentifier = GetArgument(2);
			Text = GetArgument(3);

			OnNmeaMessageParsed(this);
		}

		public override string ToString()
		{
			var start = string.Join(",",
				NmeaParser.GetSentenceStart(this),
				TotalNumberOfSentences.ToString("00"),
				SentenceNumber.ToString("00"),
				TextIdentifier,
				Text
			);

			return $"{start}*{Checksum}";
		}

		#endregion
	}
}