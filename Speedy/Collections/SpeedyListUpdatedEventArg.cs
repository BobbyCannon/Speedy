#region References

using System;
using System.Collections;

#endregion

namespace Speedy.Collections;

/// <inheritdoc />
public class SpeedyListUpdatedEventArg : EventArgs
{
	#region Constructors

	/// <summary>
	/// Represents changes to a SpeedyList.
	/// </summary>
	/// <param name="added"> The items added. </param>
	/// <param name="removed"> The items removed. </param>
	public SpeedyListUpdatedEventArg(IList added, IList removed)
	{
		Added = added;
		Removed = removed;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The items added.
	/// </summary>
	public IList Added { get; }

	/// <summary>
	/// The items removed.
	/// </summary>
	public IList Removed { get; }

	#endregion
}