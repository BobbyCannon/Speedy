#region References

using Speedy.Data;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// The extensions for <see cref="IHierarchyListItem" />
/// </summary>
public static class HierarchyListItemExtensions
{
	#region Methods

	/// <summary>
	/// Determine if the provided item is a descendant of the provided parent.
	/// </summary>
	/// <param name="item"> The item to process. </param>
	/// <param name="parent"> The parent to process. </param>
	/// <returns> True if the child is a descendant of the parent. </returns>
	public static bool IsDescendantOf(this IHierarchyListItem item, IHierarchyListItem parent)
	{
		foreach (var child in parent.GetChildren())
		{
			if (item == child)
			{
				return true;
			}

			if (IsDescendantOf(item, child))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Determine if the provided item is a descendant of the provided parent.
	/// </summary>
	/// <param name="item"> The item to process. </param>
	/// <param name="parent"> The parent to process. </param>
	/// <returns> True if the child is a descendant of the parent. </returns>
	public static bool IsDirectDescendantOf(this IHierarchyListItem item, IHierarchyListItem parent)
	{
		foreach (var child in parent.GetChildren())
		{
			if (item == child)
			{
				return true;
			}
		}

		return false;
	}

	#endregion
}