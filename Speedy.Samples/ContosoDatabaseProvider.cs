#region References

using System;

#endregion

namespace Speedy.Samples
{
	public class ContosoDatabaseProvider : IContosoDatabaseProvider
	{
		#region Fields

		private readonly string _directory;
		private readonly Lazy<ContosoDatabase> _memoryDatabase;

		#endregion

		#region Constructors

		public ContosoDatabaseProvider(string directory = null, DatabaseOptions options = null)
		{
			Options = options?.DeepClone() ?? new DatabaseOptions();
			
			_directory = directory;
			_memoryDatabase = new Lazy<ContosoDatabase>(() => new ContosoDatabase(null, Options.DeepClone()), true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		public DatabaseOptions Options { get; }

		#endregion

		#region Methods

		public IContosoDatabase GetDatabase()
		{
			if (!string.IsNullOrEmpty(_directory))
			{
				return new ContosoDatabase(_directory, Options.DeepClone());
			}

			// todo: do this better. Basically we don't want this to change.
			_memoryDatabase.Value.Options.MaintainDates = Options.MaintainDates;
			_memoryDatabase.Value.Options.MaintainSyncId = Options.MaintainSyncId;
			_memoryDatabase.Value.Options.UnmaintainEntities = Options.UnmaintainEntities;
			return _memoryDatabase.Value;
		}
		
		#endregion
	}
}