#region References

using System.Collections.Specialized;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for the NameCollectionValue type.
/// </summary>
public static class NameValueCollectionExtensions
{
	#region Methods

	/// <summary>
	/// Tries to get the values associated with the specified key from the <see cref='System.Collections.Specialized.NameValueCollection' />
	/// combined into one comma-separated list.
	/// </summary>
	/// <param name="collection"> The collection to use. </param>
	/// <param name="name"> The name of the values to read. </param>
	/// <param name="value"> The value read. Will be null if the value don't exist. </param>
	/// <returns> True if the value was read otherwise false. </returns>
	public static bool TryGet(this NameValueCollection collection, string name, out string value)
	{
		value = collection.Get(name);
		return value != null;
	}

	#endregion
}