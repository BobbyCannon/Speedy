#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberMap : EntityTypeConfiguration<GroupMember>
	{
		#region Constructors

		public GroupMemberMap()
		{
			ToTable("GroupMembers", "dbo");
			HasKey(x => x.Id);

			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.GroupId).HasColumnName("GroupId").HasColumnType("int").IsRequired();
			Property(x => x.GroupSyncId).HasColumnName("GroupSyncId").HasColumnType("uniqueidentifier").IsRequired();
			Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.MemberId).HasColumnName("MemberId").HasColumnType("int").IsRequired();
			Property(x => x.MemberSyncId).HasColumnName("MemberSyncId").HasColumnType("uniqueidentifier").IsRequired();
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Role).HasColumnName("Role").HasColumnType("nvarchar").IsRequired().HasMaxLength(4000);

			HasRequired(x => x.Group).WithMany(x => x.GroupMembers).HasForeignKey(x => x.GroupId).WillCascadeOnDelete(true);
			HasRequired(x => x.Member).WithMany(x => x.Members).HasForeignKey(x => x.MemberId).WillCascadeOnDelete(true);
		}

		#endregion
	}
}