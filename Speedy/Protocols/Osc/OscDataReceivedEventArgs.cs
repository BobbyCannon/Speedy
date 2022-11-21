#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public class OscDataReceivedEventArgs
{
	#region Constructors

	public OscDataReceivedEventArgs(byte[] buffer, int length)
	{
		Buffer = buffer;
		Length = length;
	}

	#endregion

	#region Properties

	public byte[] Buffer { get; }

	public int Length { get; }

	#endregion
}