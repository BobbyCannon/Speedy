namespace Speedy.Mobile.Models
{
	public enum MenuItemType
	{
		Browse,
		About
	}

	public class HomeMenuItem
	{
		#region Properties

		public MenuItemType Id { get; set; }

		public string Title { get; set; }

		#endregion
	}
}