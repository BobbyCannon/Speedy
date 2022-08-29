#region References

using Speedy.Data.Client;
using Speedy.EntityFramework;
using Speedy.Extensions;

#endregion

namespace Speedy.Client.Data
{
	/// <summary>
	/// Represents a model database that would be a client side representation of their data model.
	/// </summary>
	public class ContosoClientMemoryDatabase : Database, IContosoClientDatabase
	{
		#region Constructors

		public ContosoClientMemoryDatabase() : this(null, null)
		{
		}

		public ContosoClientMemoryDatabase(DatabaseOptions options, DatabaseKeyCache keyCache)
			: base(options ?? GetDefaultOptions(), keyCache)
		{
			Accounts = GetSyncableRepository<ClientAccount, int>();
			Addresses = GetSyncableRepository<ClientAddress, long>();
			LogEvents = GetSyncableRepository<ClientLogEvent, long>();
			Settings = GetSyncableRepository<ClientSetting, long>();

			this.ConfigureModelViaMapping();
		}

		#endregion

		#region Properties

		public ISyncableRepository<ClientAccount, int> Accounts { get; }

		public ISyncableRepository<ClientAddress, long> Addresses { get; }

		public ISyncableRepository<ClientLogEvent, long> LogEvents { get; }

		public ISyncableRepository<ClientSetting, long> Settings { get; }

		#endregion

		#region Methods

		public static DatabaseOptions GetDefaultOptions()
		{
			return new DatabaseOptions
			{
				SyncOrder = new[]
				{
					typeof(ClientAddress).ToAssemblyName(),
					typeof(ClientAccount).ToAssemblyName(),
					typeof(ClientLogEvent).ToAssemblyName(),
					typeof(ClientSetting).ToAssemblyName()
				}
			};
		}

		#endregion
	}
}