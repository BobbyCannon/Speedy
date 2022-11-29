#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// Represents a key for an exclusion combination
/// </summary>
public class ExclusionKey : IComparable<ExclusionKey>, IComparable, IEquatable<ExclusionKey>
{
	#region Constructors

	/// <summary>
	/// Instantiates an exclusion key
	/// </summary>
	/// <param name="type"> The type being processed. </param>
	/// <param name="excludeIncomingSync"> Should exclude properties during incoming sync. </param>
	/// <param name="excludeOutgoingSync"> Should exclude properties during outgoing sync. </param>
	/// <param name="excludeSyncUpdate"> Should exclude properties during sync update. </param>
	public ExclusionKey(Type type, bool excludeIncomingSync, bool excludeOutgoingSync, bool excludeSyncUpdate)
	{
		Type = type;
		ExcludeIncomingSync = excludeIncomingSync;
		ExcludeOutgoingSync = excludeOutgoingSync;
		ExcludeSyncUpdate = excludeSyncUpdate;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Should exclude properties during incoming sync.
	/// </summary>
	public bool ExcludeIncomingSync { get; }

	/// <summary>
	/// Should exclude properties during outgoing sync.
	/// </summary>
	public bool ExcludeOutgoingSync { get; }

	/// <summary>
	/// Should exclude properties during sync update.
	/// </summary>
	public bool ExcludeSyncUpdate { get; }

	/// <summary>
	/// The type of object this key is for.
	/// </summary>
	public Type Type { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public int CompareTo(ExclusionKey other)
	{
		if (other == null)
		{
			return -1;
		}

		if (Equals(other))
		{
			return 0;
		}

		return -1;
	}

	/// <inheritdoc />
	public int CompareTo(object obj)
	{
		return CompareTo(obj as ExclusionKey);
	}

	/// <inheritdoc />
	public bool Equals(ExclusionKey other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return (Type?.FullName?.Equals(other.Type.FullName) == true)
			&& (ExcludeIncomingSync == other.ExcludeIncomingSync)
			&& (ExcludeOutgoingSync == other.ExcludeOutgoingSync)
			&& (ExcludeSyncUpdate == other.ExcludeSyncUpdate);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj))
		{
			return false;
		}
		if (ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((ExclusionKey) obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Type.GetHashCode();
			hashCode = (hashCode * 397) ^ ExcludeIncomingSync.GetHashCode();
			hashCode = (hashCode * 397) ^ ExcludeOutgoingSync.GetHashCode();
			hashCode = (hashCode * 397) ^ ExcludeSyncUpdate.GetHashCode();
			return hashCode;
		}
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Type}, I:{ExcludeIncomingSync}, O:{ExcludeOutgoingSync}, U:{ExcludeSyncUpdate}";
	}

	#endregion
}