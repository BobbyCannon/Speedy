#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages;

/// <summary>
/// Represents a GST message.
/// </summary>
public class GstMessage : NmeaMessage
{
	#region Constructors

	public GstMessage() : base(NmeaMessageType.GST)
	{
	}

	#endregion

	#region Methods

	public override void Parse(string sentence)
	{
	}

	#endregion
}