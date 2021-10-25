#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// The sync direction.
	/// </summary>
	[Flags]
	public enum SyncDirection
	{
		/// <summary>
		/// Sync content by pulling from the Server then push content from the Client.
		/// </summary>
		PullDownThenPushUp = 0x11,

		/// <summary>
		/// Sync content by pulling from the Server down to the Client.
		/// </summary>
		PullDown = 0x01,

		/// <summary>
		/// Sync content by pushing content from the Client up to the Server.
		/// </summary>
		PushUp = 0x10
	}
}