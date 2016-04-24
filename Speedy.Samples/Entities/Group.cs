#region References

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class Group : SyncEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Group()
		{
			Members = new Collection<GroupMember>();
			IgnoreProperties.Add(nameof(Members));
		}

		#endregion

		#region Properties

		public string Description { get; set; }

		public virtual ICollection<GroupMember> Members { get; set; }

		public string Name { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Updates the relation ids using the sync ids.
		/// </summary>
		public override void UpdateLocalRelationships(ISyncableDatabase database)
		{
			// Nothing to do because group has no other direct relationships.
		}

		/// <summary>
		/// Updates the sync ids using relationships.
		/// </summary>
		public override void UpdateLocalSyncIds()
		{
			// Nothing to do because group has no other direct relationships.
		}

		#endregion
	}
}