#region References

#endregion

namespace Speedy.Samples.Entities
{
	public class FoodRelationship : IncrementingModifiableEntity
	{
		#region Properties

		public virtual Food Child { get; set; }
		public int ChildId { get; set; }
		public override int Id { get; set; }
		public virtual Food Parent { get; set; }
		public int ParentId { get; set; }
		public decimal Quantity { get; set; }

		#endregion
	}
}