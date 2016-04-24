#region References

using System;

#endregion

namespace Speedy.Samples
{
	public class ContosoDatabaseProvider : IContosoDatabaseProvider
	{
		#region Fields

		private readonly string _directory;

		#endregion

		#region Constructors

		public ContosoDatabaseProvider(string directory = null)
		{
			_directory = directory;
			MemoryDatabase = new Lazy<ContosoDatabase>(() => new ContosoDatabase(), true);
		}

		#endregion

		#region Properties

		private Lazy<ContosoDatabase> MemoryDatabase { get; }

		#endregion

		#region Methods

		public IContosoDatabase GetDatabase()
		{
			return string.IsNullOrEmpty(_directory)
				? MemoryDatabase.Value
				: new ContosoDatabase(_directory);
		}

		#endregion
	}
}