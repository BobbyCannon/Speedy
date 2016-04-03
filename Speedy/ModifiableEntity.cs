#region References

using System;

#endregion

namespace Speedy
{
	public abstract class ModifiableEntity : Entity
	{
		#region Properties

		public DateTime ModifiedOn { get; set; }

		#endregion
	}
}