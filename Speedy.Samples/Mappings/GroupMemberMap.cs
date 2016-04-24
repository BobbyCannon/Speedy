#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberMap : EntityTypeConfiguration<GroupMember>
	{
		#region Constructors

		public GroupMemberMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("GroupMembers");
			Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(t => t.GroupSyncId).IsRequired();
			Property(t => t.MemberSyncId).IsRequired();
			Property(t => t.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = false } }));
			Property(t => t.Role).IsRequired();

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