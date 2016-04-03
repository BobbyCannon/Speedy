#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	public abstract class Entity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was created.
		/// </summary>
		public DateTime CreatedOn { get; set; }

		/// <summary>
		/// Gets or sets the ID of the entity.
		/// </summary>
		public int Id { get; set; }

		#endregion
	}
}