#region References

using System;

#endregion

namespace Speedy
{
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
}