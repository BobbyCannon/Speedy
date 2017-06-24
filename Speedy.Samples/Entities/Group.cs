#region References

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Samples.Entities
{
	public class Group : IncrementingModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Group()
		{
			Members = new Collection<GroupMember>();
			//IgnoreProperties.Add(nameof(Members));
		}

		#endregion

		#region Properties

		public string Description { get; set; }

		public override int Id { get; set; }

		public virtual ICollection<GroupMember> Members { get; set; }

		public string Name { get; set; }

		#endregion
	}
}