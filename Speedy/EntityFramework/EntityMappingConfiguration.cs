#region References

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace Speedy.EntityFramework
{
	/// <inheritdoc />
	public abstract class EntityMappingConfiguration<T> : IEntityMappingConfiguration<T> where T : class
	{
		#region Methods

		/// <inheritdoc />
		public abstract void Map(EntityTypeBuilder<T> builder);

		/// <inheritdoc />
		public void Map(ModelBuilder builder)
		{
			Map(builder.Entity<T>());
		}

		#endregion
	}
}