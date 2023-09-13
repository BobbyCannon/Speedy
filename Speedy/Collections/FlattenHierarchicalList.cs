#region References

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Speedy.Data;

#endregion

namespace Speedy.Collections;

/// <summary>
/// Represents a hierarchy of data from a root collection.
/// </summary>
/// <remarks>
/// - still need to fix ordering
/// - optimize pause ordering while other sub list is ordering
/// </remarks>
public class FlattenHierarchicalList : SpeedyList<IHierarchyListItem>, IEventSubscriber
{
	#region Fields

	private bool _hasNewItemsToOrder;
	private readonly ISpeedyList _rootList;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public FlattenHierarchicalList(ISpeedyList rootList) : this(rootList, null)
	{
	}

	/// <inheritdoc />
	public FlattenHierarchicalList(ISpeedyList rootList, IDispatcher dispatcher) : base(dispatcher)
	{
		_rootList = rootList;
		_rootList.CollectionChanged += RootListOnCollectionChanged;

		DistinctCheck = Equals;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Enable to allow parent and child collections to be managed.
	/// Ex. Removing a child will clear its parent.
	///		Adding a child to a parent assigns the parent to the child.
	///		Adding a child to a different parent will remove child from old parent.
	/// </summary>
	public bool ManageRelationships { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public void CleanupEventSubscriptions()
	{
		_rootList.CollectionChanged -= RootListOnCollectionChanged;
	}

	/// <summary>
	/// Refresh the root elements ShouldBeShown and ShouldShowChildren.
	/// </summary>
	public void Refresh()
	{
		ProcessThenOrder(() =>
		{
			foreach (IHierarchyListItem item in _rootList)
			{
				item.OnShouldBeShownChanged();
				item.OnShouldShowChildrenChanged();
			}
		});
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		CleanupEventSubscriptions();
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Move)
		{
			base.OnCollectionChanged(e);
			return;
		}

		_hasNewItemsToOrder = true;

		base.OnCollectionChanged(e);
	}

	/// <inheritdoc />
	protected override bool ShouldOrder()
	{
		// Exclude order by because it's not used.
		return !IsLoading
			&& !PauseOrdering
			&& !IsOrdering
			&& (Count > 0)
			&& _hasNewItemsToOrder;
	}

	/// <inheritdoc />
	internal override void InternalOrder()
	{
		if (!ShouldOrder())
		{
			return;
		}

		IsOrdering = true;

		try
		{
			Profiler?.OrderCount.Increment();
			var orderedList = new List<IHierarchyListItem>();

			GetItemsInOrder(ref orderedList, _rootList.Cast<IHierarchyListItem>());

			for (var i = 0; i < orderedList.Count; i++)
			{
				var currentItem = orderedList[i];
				var index = InternalIndexOf(currentItem);

				if ((index != -1) && (index != i))
				{
					InternalMove(index, i);
				}
			}
		}
		finally
		{
			_hasNewItemsToOrder = false;
			IsOrdering = false;
		}
	}

	private void AttachEvents(IHierarchyListItem hierarchyItem)
	{
		hierarchyItem.ParentChanged += HierarchyItemOnParentChanged;
		hierarchyItem.ChildAdded += HierarchyItemOnChildAdded;
		hierarchyItem.ChildRemoved += HierarchyItemOnChildRemoved;
		hierarchyItem.ShouldBeShownChanged += HierarchyItemShouldBeShownChanged;
		hierarchyItem.ShouldShowChildrenChanged += HierarchyItemShouldShowChildrenChanged;
	}

	private void GetItemsInOrder(ref List<IHierarchyListItem> objects, IEnumerable<IHierarchyListItem> itemsToAdd)
	{
		if (itemsToAdd == null)
		{
			return;
		}

		foreach (var item in itemsToAdd)
		{
			if (item.ShouldBeShownAsChild())
			{
				objects.Add(item);
			}

			GetItemsInOrder(ref objects, item.GetChildren());
		}
	}

	private void HierarchyItemOnChildAdded(object sender, IHierarchyListItem e)
	{
		if (ManageRelationships)
		{
			var parent = (IHierarchyListItem) sender;
			e.UpdateParent(parent);
		}

		RequestAdd(e);
	}

	private void HierarchyItemOnChildRemoved(object sender, IHierarchyListItem e)
	{
		RequestRemove(e);

		if (ManageRelationships)
		{
			var parent = (IHierarchyListItem) sender;
			e.DisconnectParent(parent);
		}
	}

	private void HierarchyItemOnParentChanged(object sender, IHierarchyListItem oldParent)
	{
		if (!ManageRelationships)
		{
			return;
		}

		var child = (IHierarchyListItem) sender;
		oldParent?.RemoveChild(child);
	}

	private void HierarchyItemShouldBeShownChanged(object sender, EventArgs e)
	{
		if (sender is not IHierarchyListItem hierarchyItem)
		{
			return;
		}

		ProcessThenOrder(() =>
		{
			var contains = Contains(hierarchyItem);
			var shouldBeShown = hierarchyItem.ShouldBeShownAsChild();

			if (!contains && shouldBeShown)
			{
				Add(hierarchyItem);
			}
			else if (contains && !shouldBeShown)
			{
				Remove(hierarchyItem);
			}
		});
	}

	private void HierarchyItemShouldShowChildrenChanged(object sender, EventArgs e)
	{
		if (sender is not IHierarchyListItem parent
			|| !parent.HasChildren())
		{
			return;
		}

		ProcessThenOrder(() =>
		{
			foreach (var child in parent.GetChildren())
			{
				HierarchyItemShouldBeShownChanged(child, EventArgs.Empty);
			}
		});
	}

	private void RemoveEvents(IHierarchyListItem hierarchyItem)
	{
		hierarchyItem.ParentChanged -= HierarchyItemOnParentChanged;
		hierarchyItem.ChildAdded -= HierarchyItemOnChildAdded;
		hierarchyItem.ChildRemoved -= HierarchyItemOnChildRemoved;
		hierarchyItem.ShouldBeShownChanged -= HierarchyItemShouldBeShownChanged;
		hierarchyItem.ShouldShowChildrenChanged -= HierarchyItemShouldShowChildrenChanged;
	}

	private void RequestAdd(IHierarchyListItem hierarchyItem)
	{
		ProcessThenOrder(() =>
		{
			if (hierarchyItem.ShouldBeShownAsChild() && (IndexOf(hierarchyItem) == -1))
			{
				Add(hierarchyItem);
			}

			AttachEvents(hierarchyItem);

			foreach (var child in hierarchyItem.GetChildren())
			{
				RequestAdd(child);
			}
		});
	}

	private void RequestRemove(IHierarchyListItem hierarchyItem)
	{
		RemoveEvents(hierarchyItem);

		var childList = hierarchyItem.GetChildren();

		// Make sure the child list is not a disposed child
		if (childList is not ISpeedyList { IsDisposed: true })
		{
			foreach (var child in childList)
			{
				RequestRemove(child);
			}
		}

		// Finally remove, do this last because the item may dispose of itself and child resources (list, etc).
		Remove(hierarchyItem);
	}

	private void RootListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Move)
		{
			// todo: need to reorder...
			return;
		}

		if (e.OldItems != null)
		{
			foreach (var item in e.OldItems)
			{
				if (item is IHierarchyListItem hierarchyItem)
				{
					RequestRemove(hierarchyItem);
				}
				else
				{
					// Will always remove due to not being able to determine state or detect changes in state.
					Remove(item);
				}
			}
		}

		if (e.NewItems != null)
		{
			foreach (var item in e.NewItems)
			{
				if (item is IHierarchyListItem hierarchyItem)
				{
					RequestAdd(hierarchyItem);
				}
				else
				{
					// Will always show due to not being able to determine state or detect changes in state.
					Add(item);
				}
			}
		}
	}

	#endregion
}