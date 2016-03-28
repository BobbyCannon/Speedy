#region References

using System;

#endregion

namespace Speedy
{
	[Serializable]
	public abstract class Entity
	{
		#region Properties

		public DateTime CreatedOn { get; set; }
		public int Id { get; set; }
		public DateTime ModifiedOn { get; set; }

		#endregion
	}
}