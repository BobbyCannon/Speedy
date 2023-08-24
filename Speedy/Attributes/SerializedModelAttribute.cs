#region References

using System;

#endregion

namespace Speedy.Attributes;

/// <summary>
/// Attribute for ensuring a serialized class won't change and
/// break code that have serialized the object.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SerializedModelAttribute : Attribute
{
	#region Constructors

	/// <summary>
	/// Instantiates an attribute for helping test serialized objects.
	/// </summary>
	/// <param name="expected"> Expected list of serialized members. </param>
	public SerializedModelAttribute(params string[] expected)
	{
		Expected = expected;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Members to be
	/// </summary>
	public string[] Expected { get; }

	#endregion
}