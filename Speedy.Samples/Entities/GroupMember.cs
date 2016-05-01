#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class GroupMember : SyncEntity
	{
		#region Constructors

		public GroupMember()
		{
			IgnoreProperties.AddRange(nameof(Group), nameof(GroupSyncId), nameof(Member), nameof(MemberSyncId));
		}

		#endregion

		#region Properties

		public virtual Group Group { get; set; }
		public int GroupId { get; set; }
		public Guid GroupSyncId { get; set; }
		public virtual Person Member { get; set; }
		public int MemberId { get; set; }
		public Guid MemberSyncId { get; set; }
		public string Role { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Updates the sync ids using relationships.
		/// </summary>
		public override void UpdateLocalSyncIds()
		{
			this.UpdateIf(() => Group != null && Group.SyncId != GroupSyncId, () => GroupSyncId = Group.SyncId);
			this.UpdateIf(() => Member != null && Member.SyncId != MemberSyncId, () => MemberSyncId = Member.SyncId);
		}

		#endregion
	}
}