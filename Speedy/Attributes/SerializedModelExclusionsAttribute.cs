#region References

using System;

#endregion

namespace Speedy.Attributes;

/// <summary>
/// Attribute for excluding members from a serialized class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SerializedModelExclusionsAttribute : Attribute
{
	#region Constructors

	/// <summary>
	/// Instantiates an attribute for helping test serialized objects.
	/// </summary>
	/// <param name="exclusions"> Properties to exclude from testing of serialized members. </param>
	public SerializedModelExclusionsAttribute(params string[] exclusions)
	{
		Exclusions = exclusions;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Members to be excluded.
	/// </summary>
	public string[] Exclusions { get; }

	#endregion
}