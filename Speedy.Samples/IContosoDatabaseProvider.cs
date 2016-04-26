namespace Speedy.Samples
{
	public interface IContosoDatabaseProvider : ISyncableDatabaseProvider
	{
		#region Methods

		new IContosoDatabase GetDatabase();

		#endregion
	}
}