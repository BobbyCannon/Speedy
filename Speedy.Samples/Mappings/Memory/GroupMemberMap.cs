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
			database.Property<GroupMember, int>(x => x.CreatedOn).IsRequired();
			database.Property<GroupMember, int>(x => x.GroupId).IsRequired();
			database.Property<GroupMember, int>(x => x.GroupSyncId).IsRequired();
			database.Property<GroupMember, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<GroupMember, int>(x => x.MemberId).IsRequired();
			database.Property<GroupMember, int>(x => x.MemberSyncId).IsRequired();
			database.Property<GroupMember, int>(x => x.ModifiedOn).IsRequired();
			database.Property<GroupMember, int>(x => x.Role).IsRequired().HasMaximumLength(4000);
			database.HasRequired<GroupMember, Group, int>(x => x.Group, x => x.GroupId, x => x.Members);
			database.HasRequired<GroupMember, Person, int>(x => x.Member, x => x.MemberId, x => x.Groups);
		}

		#endregion
	}
}