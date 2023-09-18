namespace Speedy.Data.Views;

public class UtilityView : HierarchyListItem
{
	#region Constructors

	public UtilityView() : this(null)
	{
	}

	public UtilityView(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	public bool IsActive { get; set; }

	public UtilityType Type { get; set; }

	#endregion

	#region Methods

	public override bool ShouldBeShown()
	{
		return IsActive && (Type != UtilityType.Unknown) && base.ShouldBeShown();
	}

	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(IsActive):
			case nameof(Type):
			{
				OnShouldBeShownChanged();
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	#endregion
}

public enum UtilityType
{
	Unknown = 0,
	Electric = 1,
	Gas = 2,
	Water = 3
}