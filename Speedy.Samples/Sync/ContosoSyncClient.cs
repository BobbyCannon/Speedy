#region References

using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public class ContosoSyncClient : SyncClient, IContosoSyncClient
	{
		#region Constructors

		public ContosoSyncClient(string name, IContosoDatabaseProvider provider)
			: base(name, provider)
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