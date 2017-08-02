#region References

using System;

#endregion

namespace Speedy.Samples.Entities
{
	public class GroupMember : IncrementingModifiableEntity
	{
		#region Properties

		public virtual Group Group { get; set; }
		public int GroupId { get; set; }
		public Guid GroupSyncId { get; set; }
		public override int Id { get; set; }
		public virtual Person Member { get; set; }
		public int MemberId { get; set; }
		public Guid MemberSyncId { get; set; }
		public string Role { get; set; }

		#endregion
	}
}