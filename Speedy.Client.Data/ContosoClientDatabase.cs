#region References

using Speedy.Data.Client;

#endregion

namespace Speedy.Client.Data
{
	/// <summary>
	/// Represents a model database that would be a client side representation of their data model.
	/// </summary>
	public class ContosoClientDatabase : Database
	{
		#region Constructors

		public ContosoClientDatabase() : this(null)
		{
		}

		public ContosoClientDatabase(DatabaseOptions options) : base(options)
		{
			Addresses = GetSyncableRepository<ClientAddress, long>();
			People = GetSyncableRepository<ClientAccount, int>();

			Options.SyncOrder = new[] { typeof(ClientAddress).ToAssemblyName(), typeof(ClientAccount).ToAssemblyName() };

			// This is our only mapping
			HasRequired<ClientAccount, int, ClientAddress, long>(true, x => x.Address, x => x.AddressId, x => x.Accounts);
		}

		#endregion

		#region Properties

		public IRepository<ClientAddress, long> Addresses { get; }

		public IRepository<ClientAccount, int> People { get; }

		#endregion
	}
}