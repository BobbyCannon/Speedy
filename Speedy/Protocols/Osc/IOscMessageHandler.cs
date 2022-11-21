#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public interface IOscMessageHandler
{
	#region Properties

	OscAddress Address { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Get the object this handler will process
	/// </summary>
	/// <param name="message"> </param>
	/// <returns> </returns>
	OscCommand GetModel(OscMessage message);

	bool Matches(string address);

	bool Matches(OscMessage message);

	bool Process(object sender, OscMessage message);

	#endregion
}