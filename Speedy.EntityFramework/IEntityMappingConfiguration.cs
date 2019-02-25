#region References

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace Speedy.EntityFramework
{
	/// <inheritdoc />
	public interface IEntityMappingConfiguration<T> : IEntityMappingConfiguration where T : class
	{
		#region Methods

		/// <summary>
		/// Maps configuration for an entity.
		/// </summary>
		/// <param name="builder"> The builder to use for the mapping. </param>
		void Map(EntityTypeBuilder<T> builder);

		#endregion
	}

	/// <summary>
	/// Represents mapping of an entity.
	/// </summary>
	public interface IEntityMappingConfiguration
	{
		#region Methods

		/// <summary>
		/// Maps configuration for an entity.
		/// </summary>
		/// <param name="builder"> The builder to use for the mapping. </param>
		object Map(ModelBuilder builder);

		#endregion
	}
}