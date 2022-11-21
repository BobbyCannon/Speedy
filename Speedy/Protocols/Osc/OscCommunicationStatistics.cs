#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public class OscCommunicationStatistics : OscStatistics
{
	#region Constructors

	public OscCommunicationStatistics()
	{
		BundlesReceived = Add(new OscStatisticValue("Bundles Received"));
		BundlesSent = Add(new OscStatisticValue("Bundles Sent"));
		BytesReceived = Add(new OscStatisticValue("Bytes Received"));
		BytesSent = Add(new OscStatisticValue("Bytes Sent"));
		ExtendedBundlesReceived = Add(new OscStatisticValue("Extended Bundles Received"));
		ExtendedBundlesSent = Add(new OscStatisticValue("Extended Bundles Sent"));
		MessagesReceived = Add(new OscStatisticValue("Messages Received"));
		MessagesSent = Add(new OscStatisticValue("Messages Sent"));
		PacketsReceived = Add(new OscStatisticValue("Packets Received"));
		PacketsSent = Add(new OscStatisticValue("Packets Sent"));
		ReceiveErrors = Add(new OscStatisticValue("Receive Errors"));
	}

	#endregion

	#region Properties

	public OscStatisticValue BundlesReceived { get; }
	public OscStatisticValue BundlesSent { get; }
	public OscStatisticValue BytesReceived { get; }
	public OscStatisticValue BytesSent { get; }
	public OscStatisticValue ExtendedBundlesReceived { get; }
	public OscStatisticValue ExtendedBundlesSent { get; }
	public OscStatisticValue MessagesReceived { get; }
	public OscStatisticValue MessagesSent { get; }
	public OscStatisticValue PacketsReceived { get; }
	public OscStatisticValue PacketsSent { get; }
	public OscStatisticValue ReceiveErrors { get; }

	#endregion
}