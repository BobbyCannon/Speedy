#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.EntityFramework
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberMap : EntityMappingConfiguration<GroupMember>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<GroupMember> b)
		{
			b.ToTable("GroupMembers", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.GroupId).HasColumnName("GroupId").HasColumnType("int").IsRequired();
			b.Property(x => x.GroupSyncId).HasColumnName("GroupSyncId").HasColumnType("uniqueidentifier").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
			b.Property(x => x.MemberId).HasColumnName("MemberId").HasColumnType("int").IsRequired();
			b.Property(x => x.MemberSyncId).HasColumnName("MemberSyncId").HasColumnType("uniqueidentifier").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Role).HasColumnName("Role").HasColumnType("nvarchar(4000)").IsRequired();

			b.HasIndex(x => x.GroupId).HasName("IX_GroupId");
			b.HasIndex(x => x.MemberId).HasName("IX_MemberId");

			b.HasOne(x => x.Group).WithMany(x => x.Members).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
			b.HasOne(x => x.Member).WithMany(x => x.Groups).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
		}

		#endregion
	}
}