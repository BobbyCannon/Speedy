#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
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
			// Primary Key
			HasKey(x => x.Id);

			// Table & Column Mappings
			ToTable("GroupMembers");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.GroupSyncId).IsRequired();
			Property(x => x.MemberSyncId).IsRequired();
			Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = false } }));
			Property(x => x.Role).IsRequired();

			// Relationships
			HasRequired(x => x.Group)
				.WithMany(x => x.Members)
				.HasForeignKey(x => x.GroupId)
				.WillCascadeOnDelete(true);
			HasRequired(x => x.Member)
				.WithMany(x => x.Groups)
				.HasForeignKey(x => x.MemberId)
				.WillCascadeOnDelete(true);
		}

		#endregion
	}
}