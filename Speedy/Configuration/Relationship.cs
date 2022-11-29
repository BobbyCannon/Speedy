#region References

using System;
using System.Reflection;

#endregion

namespace Speedy.Configuration;

internal class Relationship
{
	#region Properties

	/// <summary>
	/// The property information for the entity ID.
	/// </summary>
	public PropertyInfo EntityIdPropertyInfo { get; set; }

	/// <summary>
	/// The property information for the entity.
	/// </summary>
	public PropertyInfo EntityPropertyInfo { get; set; }

	public Guid? EntitySyncId { get; set; }

	public Type Type { get; set; }

	public PropertyInfo TypeIdPropertyInfo { get; internal set; }

	#endregion
}