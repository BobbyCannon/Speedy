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
		public object Map(ModelBuilder builder)
		{
			var entity = builder.Entity<T>();
			Map(entity);
			return entity;
		}

		#endregion
	}
}