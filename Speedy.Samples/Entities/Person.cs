#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	[Serializable]
	public class Person : SyncEntity
	{
		#region Properties

		public virtual Address Address { get; set; }
		public int AddressId { get; set; }
		public string Name { get; set; }

		#endregion
	}
}