#region References

using System.Linq;
using Speedy.Extensions;

#endregion


namespace Speedy.Internal;

/// <summary>
/// Generate hashcode for many items
/// </summary>
internal static class HashCodeCalculator
{
	#region Constants

	private const int _offset = unchecked((int) 2166136261);
	private const int _prime = 16777619;

	#endregion

	#region Methods

	public static int Combine(params object[] items)
	{
		return items.Aggregate(_offset, Combine);
	}

	private static int Combine(int hashCode, object value)
	{
		return (hashCode ^ GetHashCode(value)) * _prime;
	}

	private static int GetHashCode(object value)
	{
		if (value is string sValue)
		{
			return sValue.GetStableHashCode();
		}

		return value?.GetHashCode() ?? 0;
	}

	#endregion
}