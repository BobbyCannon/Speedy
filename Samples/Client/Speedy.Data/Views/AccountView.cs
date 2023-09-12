#region References

using Speedy.Collections;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.Data.Views;

public class AccountView : Account
{
	#region Constructors

	public AccountView() : this(null)
	{
	}

	public AccountView(IDispatcher dispatcher) : base(dispatcher)
	{
		Addresses = new SpeedyList<AddressView>(dispatcher, new OrderBy<AddressView>(x => x.Line1));
		IsShown = true;
		ShowChildren = true;

		UpdateChildren(Addresses);
	}

	#endregion

	#region Properties

	public SpeedyList<AddressView> Addresses { get; }

	public bool IsShown { get; set; }

	public bool ShowChildren { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool ShouldBeShownAsChild()
	{
		return IsShown && base.ShouldBeShownAsChild();
	}

	/// <inheritdoc />
	public override bool ShouldShowChildren()
	{
		return ShowChildren && base.ShouldShowChildren();
	}

	protected override void Dispose(bool disposing)
	{
		Addresses.Dispose();
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
			case nameof(ShowChildren):
			{
				OnShouldShowChildrenChanged();
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	#endregion
}