#region References

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace Speedy.EntityFramework
{
	public abstract class EntityMappingConfiguration<T> : IEntityMappingConfiguration<T> where T : class
	{
		#region Methods

		public abstract void Map(EntityTypeBuilder<T> b);

		public void Map(ModelBuilder b)
		{
			Map(b.Entity<T>());
		}

		#endregion
	}
}