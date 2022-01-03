#region References

using Speedy.Data.Client;

#endregion

namespace Speedy.Client.Data
{
	public interface IContosoClientDatabase : ISyncableDatabase
	{
		#region Properties

		IRepository<ClientAccount, int> Accounts { get; }
		IRepository<ClientAddress, long> Addresses { get; }
		IRepository<ClientLogEvent, long> LogEvents { get; }
		IRepository<ClientSetting, long> Settings { get; }

		#endregion
	}
}