#region References

using System;

#endregion

namespace Speedy.Samples.Entities
{
	[Serializable]
	public class Person : ModifiableEntity
	{
		#region Properties

		public virtual Address Address { get; set; }
		public int AddressId { get; set; }
		public string Name { get; set; }

		#endregion
	}
}