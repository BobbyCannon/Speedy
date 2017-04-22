#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity that track the date and time it was created.
	/// </summary>
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
		/// <summary>
		/// Gets or sets the date and time the entity was created.
		/// </summary>
		DateTime CreatedOn { get; set; }
	}
}