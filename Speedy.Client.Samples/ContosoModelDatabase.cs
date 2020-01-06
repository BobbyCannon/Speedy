#region References

using Speedy.Client.Samples.Models;

#endregion

namespace Speedy.Client.Samples
{
	/// <summary>
	/// Represents a model database that would be a client side representation of their data model.
	/// </summary>
	public class ContosoModelDatabase : Database
	{
		#region Constructors

		public ContosoModelDatabase() : this(null)
		{
		}

		public ContosoModelDatabase(DatabaseOptions options) : base(options)
		{
			Addresses = GetSyncableRepository<Address, long>();
			People = GetSyncableRepository<Person, int>();

			Options.SyncOrder = new[] { typeof(Address).ToAssemblyName(), typeof(Person).ToAssemblyName() };

			// This is our only mapping
			HasRequired<Person, int, Address, long>(true, x => x.Address, x => x.AddressId, x => x.People);
		}

		#endregion

		#region Properties

		public IRepository<Address, long> Addresses { get; }

		public IRepository<Person, int> People { get; }

		#endregion
	}
}