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
			Children = new List<FoodRelationship>();
			Parents = new List<FoodRelationship>();
		}

		#endregion

		#region Properties

		public virtual ICollection<FoodRelationship> Children { get; set; }
		public override int Id { get; set; }
		public string Name { get; set; }
		public virtual ICollection<FoodRelationship> Parents { get; set; }

		#endregion
	}
}