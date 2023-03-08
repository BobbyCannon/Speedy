#region References

using System.Collections.Generic;

#endregion

namespace Speedy;

/// <summary>
/// Represents exclusion provider for IUpdatable.
/// </summary>
public interface IUpdatableExclusions
{
	#region Methods

	/// <summary>
	/// Get exclusions for the updatable type.
	/// </summary>
	HashSet<string> GetUpdatableExclusions();

	#endregion
}