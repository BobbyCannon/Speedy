#region References

using System;
using System.ComponentModel.DataAnnotations;

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
		[Display(Name = "Pull Down Then Push Up", ShortName = "Down / Up")]
		PullDownThenPushUp = 0x11,

		/// <summary>
		/// Sync content by pulling from the Server down to the Client.
		/// </summary>
		[Display(Name = "Pull Down", ShortName = "Down")]
		PullDown = 0x01,

		/// <summary>
		/// Sync content by pushing content from the Client up to the Server.
		/// </summary>
		[Display(Name = "Push Up", ShortName = "Up")]
		PushUp = 0x10
	}
}