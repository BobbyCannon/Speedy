#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class GroupEntity : Entity<int>, IModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public GroupEntity()
		{
			Members = new List<GroupMemberEntity>();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		public string Description { get; set; }

		public override int Id { get; set; }

		public virtual ICollection<GroupMemberEntity> Members { get; set; }

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		public string Name { get; set; }

		#endregion
	}
}