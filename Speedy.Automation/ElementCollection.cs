#region References

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Automation;

/// <summary>
/// Represents a collection of elements.
/// </summary>
public class ElementCollection : List<Element>
{
	#region Constructors

	/// <summary>
	/// Initializes an instance of the ElementCollection class.
	/// </summary>
	/// <param name="parent"> </param>
	internal ElementCollection(ElementHost parent)
	{
		Parent = parent;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Access an element by the Full ID, ID, or Name.
	/// </summary>
	/// <param name="id"> The ID of the element. </param>
	/// <returns> The element if found or null if not found. </returns>
	public Element this[string id] => First<Element>(id, false);

	/// <summary>
	/// Gets the parent element of this element collection.
	/// </summary>
	public ElementHost Parent { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Adds items to the <see cref="ICollection{T}" />.
	/// </summary>
	/// <param name="items"> The objects to add to the <see cref="ICollection{T}" />. </param>
	/// <exception cref="NotSupportedException">
	/// The <see cref="ICollection{T}" /> is
	/// read-only.
	/// </exception>
	public void Add(params Element[] items)
	{
		foreach (var item in items)
		{
			base.Add(item);
		}
	}

	/// <summary>
	/// First a collection of element of a specific type from the collection using the provided condition.
	/// </summary>
	/// <param name="condition"> A function to test each element for a condition. </param>
	/// <returns> The child elements for the condition. </returns>
	public bool Any<T>(Func<T, bool> condition) where T : Element
	{
		var children = OfType<T>().ToList();
		var response = children.FirstOrDefault(condition);

		if (response != null)
		{
			return true;
		}

		foreach (var child in this)
		{
			response = child.FirstOrDefault(condition, true, false);
			if (response != null)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Check to see if this collection contains an element.
	/// </summary>
	/// <param name="id"> The id to search for. </param>
	/// <returns> True if the id is found, false if otherwise. </returns>
	public bool Contains(string id)
	{
		return this[id] != null;
	}

	/// <summary>
	/// First a collection of element from the collection.
	/// </summary>
	/// <returns> The child elements for the condition. </returns>
	public IEnumerable<Element> Descendants()
	{
		return Descendants<Element>(x => true);
	}

	/// <summary>
	/// First a collection of element from the collection using the provided condition.
	/// </summary>
	/// <param name="condition"> A function to test each element for a condition. </param>
	/// <returns> The child elements for the condition. </returns>
	public IEnumerable<Element> Descendants(Func<Element, bool> condition)
	{
		return Descendants<Element>(condition);
	}

	/// <summary>
	/// First a collection of element of a specific type from the collection.
	/// </summary>
	/// <returns> The child elements for the condition. </returns>
	public IEnumerable<T> Descendants<T>() where T : Element
	{
		return Descendants<T>(x => true);
	}

	/// <summary>
	/// First a collection of element of a specific type from the collection using the provided condition.
	/// </summary>
	/// <param name="condition"> A function to test each element for a condition. </param>
	/// <returns> The child elements for the condition. </returns>
	public IEnumerable<T> Descendants<T>(Func<T, bool> condition) where T : Element
	{
		foreach (var child in OfType<T>().Where(condition))
		{
			yield return child;
		}

		foreach (var child in this)
		{
			foreach (var grandChild in child.Descendants(condition))
			{
				yield return grandChild;
			}
		}
	}

	/// <summary>
	/// Get an element from the collection using the provided ID.
	/// </summary>
	/// <param name="id"> An ID of the element to get. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <remarks>
	/// The First method throws an exception if source contains no elements. To instead return a default value
	/// when the source sequence is empty, use the FirstOrDefault method.
	/// </remarks>
	/// <returns> The child element for the ID. </returns>
	public Element First(string id, bool includeDescendants = true)
	{
		return First<Element>(id, includeDescendants);
	}

	/// <summary>
	/// Get an element from the collection using the provided condition.
	/// </summary>
	/// <param name="condition"> A function to test each element for a condition. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <remarks>
	/// The First method throws an exception if source contains no elements. To instead return a default value
	/// when the source sequence is empty, use the FirstOrDefault method.
	/// </remarks>
	/// <returns> The element matching the condition. </returns>
	public Element First(Func<Element, bool> condition, bool includeDescendants = true)
	{
		return First<Element>(condition, includeDescendants);
	}

	/// <summary>
	/// Get an element from the collection using the provided ID.
	/// </summary>
	/// <param name="id"> An ID of the element to get. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <remarks>
	/// The First method throws an exception if source contains no elements. To instead return a default value
	/// when the source sequence is empty, use the FirstOrDefault method.
	/// </remarks>
	/// <returns> The child element for the ID. </returns>
	public T First<T>(string id, bool includeDescendants = true) where T : Element
	{
		return First(GetDefaultLookupPredicate<T>(id), includeDescendants);
	}

	/// <summary>
	/// Get an element from the collection using the provided condition.
	/// </summary>
	/// <param name="condition"> A function to test each element for a condition. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <remarks>
	/// The First method throws an exception if source contains no elements. To instead return a default value
	/// when the source sequence is empty, use the FirstOrDefault method.
	/// </remarks>
	/// <returns> The child element for the condition. </returns>
	public T First<T>(Func<T, bool> condition, bool includeDescendants = true) where T : Element
	{
		var response = FirstOrDefault(condition, includeDescendants);

		if (response == null)
		{
			throw new InvalidOperationException("The source sequence is empty.");
		}

		return response;
	}

	/// <summary>
	/// Get an element from the collection using the provided ID.
	/// </summary>
	/// <param name="id"> An ID of the element to get. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <returns> The child element for the ID or null if otherwise. </returns>
	public T FirstOrDefault<T>(string id, bool includeDescendants = true) where T : Element
	{
		return FirstOrDefault(GetDefaultLookupPredicate<T>(id), includeDescendants);
	}

	/// <summary>
	/// Get an element from the collection using the provided condition.
	/// </summary>
	/// <param name="condition"> A function to test each element for a condition. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <returns> The child element for the condition or null if otherwise. </returns>
	public T FirstOrDefault<T>(Func<T, bool> condition, bool includeDescendants = true) where T : Element
	{
		var children = OfType<T>().ToList();
		var response = children.FirstOrDefault(condition);

		if (!includeDescendants)
		{
			return response;
		}

		if (response != null)
		{
			return response;
		}

		foreach (var child in this)
		{
			response = child.FirstOrDefault(condition, true, false);
			if (response != null)
			{
				return response;
			}
		}

		return null;
	}

	/// <summary>
	/// Get an element from the collection using the provided condition.
	/// </summary>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <returns> The child element for the condition or null if otherwise. </returns>
	public T FirstOrDefault<T>(bool includeDescendants = true) where T : Element
	{
		var children = OfType<T>().ToList();
		var response = children.FirstOrDefault();

		if (!includeDescendants)
		{
			return response;
		}

		if (response != null)
		{
			return response;
		}

		foreach (var child in this)
		{
			response = child.FirstOrDefault<T>(true, false);
			if (response != null)
			{
				return response;
			}
		}

		return null;
	}

	/// <summary>
	/// Gets a collection of element of the provided type.
	/// </summary>
	/// <typeparam name="T"> The type of the element for the collection. </typeparam>
	/// <returns> The collection of elements of the provided type. </returns>
	public IEnumerable<T> OfType<T>() where T : Element
	{
		return this.Where(x => (x.GetType() == typeof(T)) || x is T).Cast<T>();
	}

	/// <summary>
	/// Removes an element from a collection.
	/// </summary>
	/// <param name="element"> The element to be removed. </param>
	/// <param name="includeDescendants"> The flag that determines to search descendants or not. </param>
	/// <returns> true if item is successfully removed otherwise false. This method also returns false if item was not found. </returns>
	public bool Remove<T>(T element, bool includeDescendants = true) where T : Element
	{
		var children = OfType<T>().ToList();
		if (children.Contains(element))
		{
			children.Remove(element);
			return true;
		}

		if (!includeDescendants)
		{
			return false;
		}

		foreach (var child in this)
		{
			if (child.Remove(element))
			{
				return true;
			}
		}

		return false;
	}

	private static Func<T, bool> GetDefaultLookupPredicate<T>(string id) where T : Element
	{
		return x =>
			(x.AutomationId == id)
			|| (x.FullId == id)
			|| (x.Id == id)
			|| (x.Name == id);
	}

	#endregion
}