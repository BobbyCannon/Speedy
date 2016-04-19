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
		}

		#endregion

		#region Methods

		public IContosoDatabase CreateContext()
		{
			return new ContosoDatabase(_directory);
		}

		#endregion
	}
}