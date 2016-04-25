#region References

using System;
using System.Linq;
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
		/// Updates the relation ids using the sync ids.
		/// </summary>
		public override void UpdateLocalRelationships(ISyncableDatabase database)
		{
			this.UpdateIf(() => Group != null, () =>
			{
				Group = database.GetRepository<Group>().First(x => x.SyncId == GroupSyncId);
				GroupId = Group.Id;
				return true;
			});

			this.UpdateIf(() => Member != null, () =>
			{
				Member = database.GetRepository<Person>().First(x => x.SyncId == MemberSyncId);
				MemberId = Member.Id;
				return true;
			});
		}

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