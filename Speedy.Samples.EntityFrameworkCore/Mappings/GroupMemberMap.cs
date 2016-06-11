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

			mapping.HasKey(x => x.Id);
			mapping.ToTable("GroupMembers");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.GroupSyncId).IsRequired();
			mapping.Property(x => x.MemberSyncId).IsRequired();
			mapping.Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.Role).IsRequired();
			mapping.HasOne(x => x.Group).WithMany(x => x.Members).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
			mapping.HasOne(x => x.Member).WithMany(x => x.Groups).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
		}

		#endregion
	}
}