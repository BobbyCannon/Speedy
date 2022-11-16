#region References

using Speedy.Devices.Location;
using Xamarin.Forms;

#endregion

namespace Speedy.Samples.Xamarin;

public class LocationTemplateSelector : DataTemplateSelector
{
	#region Properties

	public DataTemplate GenericTemplate { get; set; }
	public DataTemplate HorizontalTemplate { get; set; }

	public DataTemplate VerticalTemplate { get; set; }

	#endregion

	#region Methods

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		return item switch
		{
			IHorizontalLocation => HorizontalTemplate,
			IVerticalLocation => VerticalTemplate,
			_ => GenericTemplate
		};
	}

	#endregion
}