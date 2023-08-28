#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// Represents a created entity.
/// </summary>
/// <typeparam name="T"> The type of the entity key. </typeparam>
public abstract class CreatedEntity<T> : Entity<T>, ICreatedEntity
{
	#region Properties

	/// <inheritdoc />
	public DateTime CreatedOn { get; set; }

	#endregion
}

/// <summary>
/// Represents a Speedy entity that track the date and time it was created.
/// </summary>
public interface ICreatedEntity : IEntity
{
	#region Properties

	/// <summary>
	/// Gets or sets the date and time the entity was created.
	/// </summary>
	DateTime CreatedOn { get; set; }

	#endregion
}