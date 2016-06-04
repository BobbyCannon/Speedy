#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Speedy.EntityFrameworkCore;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFrameworkCore.Mappings
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberMap : IEntityTypeConfiguration
	{
		#region Constructors

		#endregion

		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<GroupMember>();

			mapping.HasKey(t => t.Id);
			mapping.ToTable("GroupMembers");
			mapping.Property(t => t.Id).UseSqlServerIdentityColumn();
			mapping.Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(t => t.GroupSyncId).IsRequired();
			mapping.Property(t => t.MemberSyncId).IsRequired();
			mapping.Property(t => t.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(t => t.Role).IsRequired();
			mapping.HasOne(x => x.Group).WithMany(x => x.Members).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
			mapping.HasOne(x => x.Member).WithMany(x => x.Groups).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
		}

		#endregion
	}
}