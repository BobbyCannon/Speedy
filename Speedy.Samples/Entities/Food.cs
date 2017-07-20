#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy;

#endregion

namespace Speedy.Samples.Entities
{
	public class Food : IncrementingModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Food()
		{
			ChildRelationships = new List<FoodRelationship>();
			ParentRelationships = new List<FoodRelationship>();
		}

		#endregion

		#region Properties

		public virtual ICollection<FoodRelationship> ChildRelationships { get; set; }
		public override int Id { get; set; }
		public string Name { get; set; }
		public virtual ICollection<FoodRelationship> ParentRelationships { get; set; }

		#endregion
	}
}