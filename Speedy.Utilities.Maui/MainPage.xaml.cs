#region References

using Speedy.Utilities.Maui.ViewModels;

#endregion

namespace Speedy.Utilities.Maui;

public partial class MainPage
{
	#region Constructors

	public MainPage(ClientViewModel model)
	{
		InitializeComponent();

		BindingContext = model;
	}

	#endregion
}