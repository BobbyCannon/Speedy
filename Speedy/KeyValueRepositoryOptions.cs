#region References

using System;

#endregion

namespace Speedy
{
	public class KeyValueRepositoryOptions
	{
		#region Properties

		public bool IgnoreVirtualMembers { get; set; }

		public int Limit { get; set; }

		public TimeSpan Timeout { get; set; }

		#endregion
	}
}