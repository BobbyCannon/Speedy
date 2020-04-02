#region References

using Speedy.Data.Client;
using Speedy.Extensions;

#endregion

namespace Speedy.Client.Data
{
	/// <summary>
	/// Represents a model database that would be a client side representation of their data model.
	/// </summary>
	public class ContosoClientMemoryDatabase : Database
	{
		#region Constructors

		public ContosoClientMemoryDatabase() : this(null)
		{
		}

		public ContosoClientMemoryDatabase(DatabaseOptions options) : base(options)
		{
			Accounts = GetSyncableRepository<ClientAccount, int>();
			Addresses = GetSyncableRepository<ClientAddress, long>();
			LogEvents = GetSyncableRepository<ClientLogEvent, long>();

			SetRequiredOptions(Options);

			// This is our only mapping
			HasRequired<ClientAccount, int, ClientAddress, long>(true, x => x.Address, x => x.AddressId, x => x.Accounts);
		}

		#endregion

		#region Properties

		public IRepository<ClientAccount, int> Accounts { get; }

		public IRepository<ClientAddress, long> Addresses { get; }

		public IRepository<ClientLogEvent, long> LogEvents { get; }

		#endregion

		#region Methods

		public static DatabaseOptions GetDefaultOptions()
		{
			var response = new DatabaseOptions();
			SetRequiredOptions(response);
			return response;
		}

		public static void SetRequiredOptions(DatabaseOptions options)
		{
			options.SyncOrder = new[] { typeof(ClientAddress).ToAssemblyName(), typeof(ClientAccount).ToAssemblyName(), typeof(ClientLogEvent).ToAssemblyName() };
		}

		#endregion
	}
}