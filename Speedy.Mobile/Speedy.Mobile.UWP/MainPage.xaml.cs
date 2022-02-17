namespace Speedy.Mobile.UWP
{
	public sealed partial class MainPage
	{
		#region Constructors

		public MainPage()
		{
			InitializeComponent();

			LoadApplication(new Mobile.App());
		}

		#endregion
	}
}