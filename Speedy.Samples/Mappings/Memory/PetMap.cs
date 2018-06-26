#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class PetMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<Pet, Pet.PetKey>(x => x.CreatedOn).IsRequired();
			database.Property<Pet, Pet.PetKey>(x => x.ModifiedOn).IsRequired();
			database.Property<Pet, Pet.PetKey>(x => x.Name).IsRequired().HasMaximumLength(128).IsUnique();
			database.Property<Pet, Pet.PetKey>(x => x.OwnerId).IsRequired().IsUnique();
			database.Property<Pet, Pet.PetKey>(x => x.TypeId).IsRequired().HasMaximumLength(25);
			database.HasRequired<Pet, Pet.PetKey, Person, int>(x => x.Owner, x => x.OwnerId, x => x.Owners);
			database.HasRequired<Pet, Pet.PetKey, PetType, string>(x => x.Type, x => x.TypeId, x => x.Types);
		}

		#endregion
	}
}