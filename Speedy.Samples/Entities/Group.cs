#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy;

#endregion

namespace Speedy.Samples.Entities
{
	public class Group : IncrementingModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Group()
		{
			Members = new List<GroupMember>();
		}

		#endregion

		#region Properties

		public string Description { get; set; }
		public override int Id { get; set; }
		public virtual ICollection<GroupMember> Members { get; set; }
		public string Name { get; set; }

		#endregion
	}
}