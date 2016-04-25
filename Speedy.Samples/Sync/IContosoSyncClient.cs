#region References

using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public interface IContosoSyncClient : ISyncClient
	{
		#region Methods

		IContosoDatabase GetDatabase();

		#endregion
	}
}