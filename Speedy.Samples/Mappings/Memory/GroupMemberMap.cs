#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<GroupMemberEntity, int>(x => x.CreatedOn).IsRequired();
			database.Property<GroupMemberEntity, int>(x => x.GroupId).IsRequired();
			database.Property<GroupMemberEntity, int>(x => x.GroupSyncId).IsRequired();
			database.Property<GroupMemberEntity, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<GroupMemberEntity, int>(x => x.MemberId).IsRequired();
			database.Property<GroupMemberEntity, int>(x => x.MemberSyncId).IsRequired();
			database.Property<GroupMemberEntity, int>(x => x.ModifiedOn).IsRequired();
			database.Property<GroupMemberEntity, int>(x => x.Role).IsRequired().HasMaximumLength(4000);
			database.HasRequired<GroupMemberEntity, int, GroupEntity, int>(x => x.Group, x => x.GroupId, x => x.Members);
			database.HasRequired<GroupMemberEntity, int, PersonEntity, int>(x => x.Member, x => x.MemberId, x => x.Groups);
		}

		#endregion
	}
}