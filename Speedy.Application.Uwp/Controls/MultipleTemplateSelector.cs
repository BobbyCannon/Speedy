#region References

using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Speedy.Collections;

#endregion

namespace Speedy.Application.Uwp.Controls;

[ContentProperty(Name = nameof(Matches))]
public class MultipleTemplateSelector : DataTemplateSelector
{
	#region Constructors

	public MultipleTemplateSelector()
	{
		Matches = new BaseObservableCollection<TemplateMatch>();
	}

	#endregion

	#region Properties

	public ObservableCollection<TemplateMatch> Matches { get; set; }

	#endregion

	#region Methods

	protected override DataTemplate SelectTemplateCore(object item)
	{
		if (item == null)
		{
			return null;
		}

		return Matches.FirstOrDefault(m => m.TargetType == item.GetType().Name)?.Template;
	}

	protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
	{
		return SelectTemplateCore(item);
	}

	#endregion
}