namespace Speedy.Samples
{
	public interface ISampleDatabaseProvider
	{
		#region Methods

		ISampleDatabase CreateContext();

		#endregion
	}
}