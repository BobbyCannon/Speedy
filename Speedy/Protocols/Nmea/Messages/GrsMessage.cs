#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages
{
	/// <summary>
	/// Represents a GRS message.
	/// </summary>
	public class GrsMessage : NmeaMessage
	{
		#region Constructors

		public GrsMessage() : base(NmeaMessageType.GRS)
		{
		}

		#endregion

		#region Methods

		public override void Parse(string sentence)
		{
		}

		#endregion
	}
}