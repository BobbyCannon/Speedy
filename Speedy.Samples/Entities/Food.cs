#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Samples.Entities
{
	public class Food : Entity
	{
		#region Constructors

		public Food()
		{
			Children = new List<FoodRelationship>();
			Parents = new List<FoodRelationship>();
		}

		#endregion

		#region Properties

		public virtual ICollection<FoodRelationship> Children { get; set; }
		public string Name { get; set; }
		public virtual ICollection<FoodRelationship> Parents { get; set; }

		#endregion
	}
}