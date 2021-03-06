﻿#region References

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
			: base(options ?? ContosoClientDatabase.GetDefaultOptions(), keyCache)
		{
			Accounts = GetSyncableRepository<ClientAccount, int>();
			Addresses = GetSyncableRepository<ClientAddress, long>();
			LogEvents = GetSyncableRepository<ClientLogEvent, long>();
			Settings = GetSyncableRepository<ClientSetting, long>();
			
			this.ConfigureModelViaMapping();
		}

		#endregion

		#region Properties

		public IRepository<ClientAccount, int> Accounts { get; }

		public IRepository<ClientAddress, long> Addresses { get; }

		public IRepository<ClientLogEvent, long> LogEvents { get; }

		public IRepository<ClientSetting, long> Settings { get; }

		#endregion
	}
}