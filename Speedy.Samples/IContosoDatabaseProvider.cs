namespace Speedy.Samples
{
	public interface IContosoDatabaseProvider
	{
		#region Methods

		IContosoDatabase CreateContext();

		#endregion
	}
}