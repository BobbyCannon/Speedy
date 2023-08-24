#region References

using System.Collections.Generic;

#endregion

namespace Speedy;

/// <summary>
/// Represents exclusion provider for IUpdateable.
/// </summary>
public interface IUpdateableExclusions
{
	#region Methods

	/// <summary>
	/// Get exclusions for the updatable type.
	/// </summary>
	HashSet<string> GetUpdatableExclusions();

	#endregion
}