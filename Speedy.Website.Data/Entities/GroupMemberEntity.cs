#region References

using System;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class GroupMemberEntity : Entity<int>, IModifiableEntity
	{
		#region Properties

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		public virtual GroupEntity Group { get; set; }

		public int GroupId { get; set; }

		public override int Id { get; set; }

		public virtual AccountEntity Member { get; set; }

		public int MemberId { get; set; }

		public Guid MemberSyncId { get; set; }

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		public string Role { get; set; }

		#endregion
	}
}