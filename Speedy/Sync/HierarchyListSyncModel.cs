#region References

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Speedy.Collections;
using Speedy.Data;

#endregion

namespace Speedy.Sync;

/// <summary>
/// Represents a sync model in a hierarchy list.
/// </summary>
/// <typeparam name="T"> The type for the sync model key. </typeparam>
public class HierarchyListSyncModel<T> : SyncModel<T>, IHierarchyListItem
{
	#region Fields

	private ISpeedyList _children;
	private IHierarchyListItem _parent;
	private bool _disposed;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public HierarchyListSyncModel(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override T Id { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual void CleanupEventSubscriptions()
	{
		DisconnectParent(_parent);
		DisconnectChildrenEvents(_children);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public IEnumerable<IHierarchyListItem> GetChildren()
	{
		if (_children != null)
		{
			return _children.Cast<IHierarchyListItem>();
		}

		return Array.Empty<IHierarchyListItem>();
	}

	/// <inheritdoc />
	public IHierarchyListItem GetParent()
	{
		return _parent;
	}

	/// <inheritdoc />
	public bool HasBeenDisposed()
	{
		return _disposed;
	}

	/// <inheritdoc />
	public bool HasChildren()
	{
		return GetChildren().Any();
	}

	/// <inheritdoc />
	public virtual bool ShouldBeShownAsChild()
	{
		var parent = GetParent();
		return (parent == null)
			|| (parent.ShouldShowChild(this)
				&& parent.ShouldShowChildren());
	}

	/// <inheritdoc />
	public virtual bool ShouldShowChild(IHierarchyListItem child)
	{
		return ShouldShowChildren();
	}

	/// <inheritdoc />
	public virtual bool ShouldShowChildren()
	{
		return true;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			CleanupEventSubscriptions();
		}

		_disposed = true;
	}

	/// <summary>
	/// Trigger the ChildAdded event.
	/// </summary>
	/// <param name="e"> The child added. </param>
	protected virtual void OnChildAdded(IHierarchyListItem e)
	{
		ChildAdded?.Invoke(this, e);
	}

	/// <summary>
	/// Trigger the ChildRemoved event.
	/// </summary>
	/// <param name="e"> The child removed. </param>
	protected virtual void OnChildRemoved(IHierarchyListItem e)
	{
		ChildRemoved?.Invoke(this, e);
	}

	/// <summary>
	/// Trigger the ParentChanged event.
	/// </summary>
	/// <param name="e"> The parent that was assigned. </param>
	protected virtual void OnParentChanged(IHierarchyListItem e)
	{
		ParentChanged?.Invoke(this, e);
	}

	/// <summary>
	/// Trigger the ShouldBeShownChanged event.
	/// </summary>
	protected virtual void OnShouldBeShownChanged()
	{
		ShouldBeShownChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Trigger the ShouldShowChildrenChanged event.
	/// </summary>
	protected virtual void OnShouldShowChildrenChanged()
	{
		ShouldShowChildrenChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Initialize the relationship. This should be called in the constructor.
	/// </summary>
	/// <param name="children"> The children for the list item. </param>
	protected void UpdateChildren(ISpeedyList children)
	{
		if (children == _children)
		{
			return;
		}

		DisconnectChildrenEvents(_children);
		_children = ConnectChildrenEvents(children);
	}

	/// <summary>
	/// Initialize the relationship.
	/// </summary>
	/// <param name="parent"> The parent of the list item. </param>
	protected void UpdateParent(IHierarchyListItem parent)
	{
		if (parent == _parent)
		{
			return;
		}

		DisconnectParent(_parent);
		_parent = ConnectParentEvents(parent);

		OnParentChanged(_parent);
	}

	private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Move)
		{
			return;
		}

		if (e.OldItems != null)
		{
			foreach (var item in e.OldItems)
			{
				var listItem = (IHierarchyListItem) item;
				listItem.ClearParent();
				listItem.CleanupEventSubscriptions();

				OnChildRemoved(listItem);
			}
		}

		if (e.NewItems != null)
		{
			foreach (var item in e.NewItems)
			{
				var listItem = (IHierarchyListItem) item;
				listItem.UpdateParent(this);

				OnChildAdded(listItem);
			}
		}
	}

	/// <inheritdoc />
	void IHierarchyListItem.ClearParent()
	{
		UpdateParent(null);
	}

	private ISpeedyList ConnectChildrenEvents(ISpeedyList children)
	{
		if (children != null)
		{
			children.CollectionChanged += ChildrenOnCollectionChanged;
		}

		return children;
	}

	private IHierarchyListItem ConnectParentEvents(IHierarchyListItem parent)
	{
		if (parent != null)
		{
			parent.ShouldBeShownChanged += ParentOnShouldBeShownChanged;
			parent.ShouldShowChildrenChanged += ParentShouldShowChildrenChanged;
		}

		return parent;
	}

	private void DisconnectChildrenEvents(ISpeedyList children)
	{
		if (children != null)
		{
			children.CollectionChanged -= ChildrenOnCollectionChanged;
		}
	}

	private void DisconnectParent(IHierarchyListItem parent)
	{
		if (parent == null)
		{
			return;
		}

		_parent = null;

		parent.ShouldBeShownChanged -= ParentOnShouldBeShownChanged;
		parent.ShouldShowChildrenChanged -= ParentShouldShowChildrenChanged;
		parent.RemoveChild(this);
	}

	/// <inheritdoc />
	void IHierarchyListItem.OnShouldBeShownChanged()
	{
		OnShouldBeShownChanged();
	}

	/// <inheritdoc />
	void IHierarchyListItem.OnShouldShowChildrenChanged()
	{
		OnShouldShowChildrenChanged();
	}

	private void ParentOnShouldBeShownChanged(object sender, EventArgs e)
	{
		OnShouldBeShownChanged();
	}

	private void ParentShouldShowChildrenChanged(object sender, EventArgs e)
	{
		OnShouldBeShownChanged();
	}

	/// <inheritdoc />
	void IHierarchyListItem.RemoveChild(IHierarchyListItem child)
	{
		if (_children?.IsDisposed == false)
		{
			_children?.Remove(child);
		}
	}

	/// <inheritdoc />
	void IHierarchyListItem.UpdateParent(IHierarchyListItem parent)
	{
		UpdateParent(parent);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<IHierarchyListItem> ChildAdded;

	/// <inheritdoc />
	public event EventHandler<IHierarchyListItem> ChildRemoved;

	/// <inheritdoc />
	public event EventHandler<IHierarchyListItem> ParentChanged;

	/// <inheritdoc />
	public event EventHandler ShouldBeShownChanged;

	/// <inheritdoc />
	public event EventHandler ShouldShowChildrenChanged;

	#endregion
}