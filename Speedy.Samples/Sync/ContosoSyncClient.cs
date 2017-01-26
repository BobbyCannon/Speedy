#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public class ContosoSyncClient : SyncClient, IContosoSyncClient
	{
		#region Constructors

		public ContosoSyncClient(string name, IContosoDatabaseProvider provider)
			: this(name, Guid.NewGuid(), provider)
		{
		}

		public ContosoSyncClient(string name, Guid sessionId, IContosoDatabaseProvider provider)
			: base(name, sessionId, provider)
		{
		}

		#endregion

		#region Methods

		public IContosoDatabase GetDatabase()
		{
			return (IContosoDatabase) DatabaseProvider.GetDatabase();
		}

		#endregion
	}
}