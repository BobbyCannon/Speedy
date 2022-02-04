#region References

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Speedy.Client.Data;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.Mobile.Services
{
	public class MockDataStore : IDataStore<ClientLogEvent>
	{
		#region Fields

		private readonly ContosoClientDatabase _database;

		#endregion

		#region Constructors

		public MockDataStore()
		{
			_database = ContosoClientDatabase.UseSqlite();
			_database.Database.EnsureCreated();

			if (!_database.LogEvents.Any())
			{
				_database.LogEvents.Add(new ClientLogEvent { Level = LogLevel.Critical, Message = "Hello World" });
				_database.LogEvents.Add(new ClientLogEvent { Level = LogLevel.Debug, Message = "Foo Bar" });
				_database.SaveChanges();
			}
		}

		#endregion

		#region Methods

		public async Task<bool> AddItemAsync(ClientLogEvent item)
		{
			_database.LogEvents.Add(new ClientLogEvent { Message = item.Message, Level = item.Level });
			_database.SaveChanges();
			return await Task.FromResult(true);
		}

		public async Task<bool> DeleteItemAsync(long id)
		{
			_database.LogEvents.Remove(id);
			_database.SaveChanges();
			return await Task.FromResult(true);
		}

		public async Task<ClientLogEvent> GetItemAsync(long id)
		{
			var log = _database.LogEvents.FirstOrDefault(x => x.Id == id);
			return await Task.FromResult((ClientLogEvent) log?.Unwrap());
		}

		public async Task<IEnumerable<ClientLogEvent>> GetItemsAsync(bool forceRefresh = false)
		{
			return await Task.FromResult(_database.LogEvents.Select(x => (ClientLogEvent) x.Unwrap()).ToList());
		}

		public async Task<bool> UpdateItemAsync(ClientLogEvent item)
		{
			return await Task.FromResult(true);
		}

		#endregion
	}
}