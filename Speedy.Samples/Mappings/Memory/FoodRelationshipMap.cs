#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class FoodRelationshipMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<FoodRelationship, int>(x => x.ChildId).IsRequired();
			database.Property<FoodRelationship, int>(x => x.CreatedOn).IsRequired();
			database.Property<FoodRelationship, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<FoodRelationship, int>(x => x.ModifiedOn).IsRequired();
			database.Property<FoodRelationship, int>(x => x.ParentId).IsRequired();
			database.Property<FoodRelationship, int>(x => x.Quantity).IsRequired();
			database.HasRequired<FoodRelationship, Food, int>(x => x.Child, x => x.ChildId, x => x.ParentRelationships);
			database.HasRequired<FoodRelationship, Food, int>(x => x.Parent, x => x.ParentId, x => x.ChildRelationships);
		}

		#endregion
	}
}