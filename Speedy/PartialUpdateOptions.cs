#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy
{
	/// <summary>
	/// Options for Partial Update
	/// </summary>
	public class PartialUpdateOptions : Bindable
	{
		#region Constructors

		/// <summary>
		/// Instantiates options for validation for a partial update.
		/// </summary>
		public PartialUpdateOptions()
		{
			ExcludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IncludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Properties to be excluded.
		/// </summary>
		public HashSet<string> ExcludedProperties { get; }

		/// <summary>
		/// Properties to be included.
		/// </summary>
		public HashSet<string> IncludedProperties { get; }

		#endregion
	}
}