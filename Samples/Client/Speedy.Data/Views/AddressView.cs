#region References

using Speedy.Collections;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.Data.Views;

public class AddressView : Address
{
	#region Constructors

	public AddressView() : this(null)
	{
	}

	public AddressView(IDispatcher dispatcher) : base(dispatcher)
	{
		IsShown = true;
		ShowUtilities = true;
		Utilities = new SpeedyList<UtilityView>(dispatcher, new OrderBy<UtilityView>(x => x.Type));

		UpdateChildren(Utilities);
	}

	#endregion

	#region Properties

	public bool IsShown { get; set; }

	public bool ShowUtilities { get; set; }

	public SpeedyList<UtilityView> Utilities { get; }

	#endregion

	#region Methods

	public override bool ShouldBeShown()
	{
		return IsShown && base.ShouldBeShown();
	}

	public override bool ShouldShowChildren()
	{
		return ShowUtilities && base.ShouldShowChildren();
	}

	protected override void Dispose(bool disposing)
	{
		Utilities.Dispose();
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(IsShown):
			{
				OnShouldBeShownChanged();
				break;
			}
			case nameof(ShowUtilities):
			{
				OnShouldShowChildrenChanged();
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	#endregion
}