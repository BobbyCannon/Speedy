#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity that track the date and time it was last modified.
	/// </summary>
	public abstract class ModifiableEntity<T> : CreatedEntity<T>, IModifiableEntity
	{
		#region Properties

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity that track the date and time it was last modified.
	/// </summary>
	public interface IModifiableEntity : ICreatedEntity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was modified.
		/// </summary>
		DateTime ModifiedOn { get; set; }

		#endregion
	}
}