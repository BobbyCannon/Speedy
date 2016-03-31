namespace Speedy.Samples
{
	public class SampleDatabaseProvider : ISampleDatabaseProvider
	{
		#region Fields

		private readonly string _directory;

		#endregion

		#region Constructors

		public SampleDatabaseProvider(string directory = null)
		{
			_directory = directory;
		}

		#endregion

		#region Methods

		public ISampleDatabase CreateContext()
		{
			return new SampleDatabase(_directory);
		}

		#endregion
	}
}