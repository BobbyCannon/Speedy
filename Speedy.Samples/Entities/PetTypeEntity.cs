#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Samples.Entities
{
	public class PetTypeEntity : Entity<string>, IModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public PetTypeEntity()
		{
			Types = new List<PetEntity>();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		public override string Id { get; set; }

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		public string Type { get; set; }
		public virtual ICollection<PetEntity> Types { get; set; }

		#endregion
	}
}