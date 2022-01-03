#region References

using Speedy.Sync;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for sync object.
	/// </summary>
	public static class SyncObjectExtensions
	{
		#region Constructors

		static SyncObjectExtensions()
		{
			Empty = new SyncObject();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Represents an empty sync object.
		/// </summary>
		public static SyncObject Empty { get; }

		#endregion
	}
}