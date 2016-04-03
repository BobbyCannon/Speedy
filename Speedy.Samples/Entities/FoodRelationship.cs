namespace Speedy.Samples.Entities
{
	public class FoodRelationship : ModifiableEntity
	{
		#region Properties

		public virtual Food Child { get; set; }
		public int ChildId { get; set; }
		public virtual Food Parent { get; set; }
		public int ParentId { get; set; }
		public decimal Quantity { get; set; }

		#endregion
	}
}