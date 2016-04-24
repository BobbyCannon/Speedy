#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public interface IContosoSyncClient : ISyncClient
	{
		#region Properties

		IContosoDatabase Database { get; }

		#endregion

		#region Methods

		IContosoDatabase GetDatabase();

		void SaveChanges();

		#endregion
	}
}