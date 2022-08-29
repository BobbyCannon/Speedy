#region References

using Speedy.Data.Client;

#endregion

namespace Speedy.Client.Data
{
	public interface IContosoClientDatabase : ISyncableDatabase
	{
		#region Properties

		ISyncableRepository<ClientAccount, int> Accounts { get; }
		ISyncableRepository<ClientAddress, long> Addresses { get; }
		ISyncableRepository<ClientLogEvent, long> LogEvents { get; }
		ISyncableRepository<ClientSetting, long> Settings { get; }

		#endregion
	}
}