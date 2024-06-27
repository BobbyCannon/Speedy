using System;

namespace Speedy.Sync;

/// <summary>
/// Client / Server Sync Times
/// </summary>
public struct SyncTimes : ISyncTimes
{
	/// <summary>
	/// Instantiates an instance of the class.
	/// </summary>
	public SyncTimes() : this(DateTime.MinValue, DateTime.MinValue)
	{
	}

	/// <summary>
	/// Instantiates an instance of the class.
	/// </summary>
	/// <param name="client"> The date and time for the client. </param>
	/// <param name="server"> The date and time for the server. </param>
	public SyncTimes(DateTime client, DateTime server)
	{
		LastSyncedOnClient = client;
		LastSyncedOnServer = server;
	}

	#region Properties

	/// <inheritdoc />
	public DateTime LastSyncedOnClient { get; set; }

	/// <inheritdoc />
	public DateTime LastSyncedOnServer { get; set; }

	#endregion
}

/// <summary>
/// Client / Server Sync Times
/// </summary>
public interface ISyncTimes
{
	#region Properties

	/// <summary>
	/// Gets or sets the last synced on date and time for the client.
	/// </summary>
	DateTime LastSyncedOnClient { get; set; }

	/// <summary>
	/// Gets or sets the last synced on date and time for the server.
	/// </summary>
	DateTime LastSyncedOnServer { get; set; }

	#endregion
}