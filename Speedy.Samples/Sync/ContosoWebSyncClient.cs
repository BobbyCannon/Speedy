#region References

using System.Threading;
using Speedy.Net;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Sync
{
	public class ContosoWebSyncClient : WebSyncClient, IContosoSyncClient
	{
		#region Fields

		private readonly IContosoDatabaseProvider _provider;

		#endregion

		#region Constructors

		public ContosoWebSyncClient(string name, IContosoDatabaseProvider provider)
			: base(name, "http://localhost")
		{
			_provider = provider;
			Database = _provider.GetDatabase();
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => Database.Addresses;

		public IContosoDatabase Database { get; }

		public IRepository<Person> People => Database.People;

		#endregion

		#region Methods

		public IContosoDatabase GetDatabase()
		{
			return _provider.GetDatabase();
		}

		public void SaveChanges()
		{
			Database.SaveChanges();
			Thread.Sleep(1);
		}

		#endregion
	}
}