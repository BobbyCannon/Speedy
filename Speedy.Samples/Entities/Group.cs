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
	}
}