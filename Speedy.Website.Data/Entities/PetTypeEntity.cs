#region References

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class PetTypeEntity : ModifiableEntity<string>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public PetTypeEntity()
		{
			Types = new List<PetEntity>();
		}

		#endregion

		#region Properties

		public override string Id { get; set; }

		public string Type { get; set; }

		public virtual ICollection<PetEntity> Types { get; set; }

		#endregion
	}
}