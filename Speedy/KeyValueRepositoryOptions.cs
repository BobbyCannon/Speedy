#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Options for a key value repository.
	/// </summary>
	public class KeyValueRepositoryOptions
	{
		#region Properties

		/// <summary>
		/// Gets or sets a flag to ignore virtual members when saving entities.
		/// </summary>
		public bool IgnoreVirtualMembers { get; set; }

		/// <summary>
		/// Gets or sets the caching limit.
		/// </summary>
		public int Limit { get; set; }

		/// <summary>
		/// Gets or sets the flag to determine if this repository is read only.
		/// </summary>
		public bool ReadOnly { get; set; }

		/// <summary>
		/// Gets or sets the maximum time to cache items.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		#endregion
	}
}