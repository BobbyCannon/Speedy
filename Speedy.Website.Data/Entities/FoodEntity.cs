#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class FoodEntity : Entity<int>, IModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public FoodEntity()
		{
			ChildRelationships = new List<FoodRelationshipEntity>();
			ParentRelationships = new List<FoodRelationshipEntity>();
		}

		#endregion

		#region Properties

		public virtual ICollection<FoodRelationshipEntity> ChildRelationships { get; set; }

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		public override int Id { get; set; }

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		public string Name { get; set; }

		public virtual ICollection<FoodRelationshipEntity> ParentRelationships { get; set; }

		#endregion
	}
}