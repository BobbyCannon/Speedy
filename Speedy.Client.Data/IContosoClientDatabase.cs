using Speedy.Data.Client;

namespace Speedy.Client.Data
{
	public interface IContosoClientDatabase
	{
		IRepository<ClientAccount, int> Accounts { get; }
		IRepository<ClientAddress, long> Addresses { get; }
		IRepository<ClientLogEvent, long> LogEvents { get; }
		IRepository<ClientSetting, long> Settings { get; }
	}
}