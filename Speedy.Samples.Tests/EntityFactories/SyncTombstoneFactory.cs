#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class SyncTombstoneFactory
	{
		#region Methods

		public static SyncTombstone Get(Action<SyncTombstone> update = null)
		{
			var result = new SyncTombstone
			{
				Id = default(long),
				ReferenceId = Guid.NewGuid().ToString(),
				SyncId = default(Guid),
				TypeName = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}