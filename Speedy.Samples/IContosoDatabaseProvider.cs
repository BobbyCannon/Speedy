namespace Speedy.Samples
{
	public interface IContosoDatabaseProvider
	{
		#region Methods

		IContosoDatabase GetDatabase();

		#endregion
	}
}