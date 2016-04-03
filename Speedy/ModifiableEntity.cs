#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity that track the date and time it was last modified.
	/// </summary>
	public abstract class ModifiableEntity : Entity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was modified.
		/// </summary>
		public DateTime ModifiedOn { get; set; }

		#endregion
	}
}