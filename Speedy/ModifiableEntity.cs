﻿#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a modifiable entity.
	/// </summary>
	/// <typeparam name="T"> The type of the entity key. </typeparam>
	public abstract class ModifiableEntity<T> : CreatedEntity<T>, IModifiableEntity
	{
		#region Properties

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		#endregion
	}
}