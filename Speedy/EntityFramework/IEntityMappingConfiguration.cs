#region References

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace Speedy.EntityFramework
{
	public interface IEntityMappingConfiguration<T> : IEntityMappingConfiguration where T : class
	{
		#region Methods

		void Map(EntityTypeBuilder<T> builder);

		#endregion
	}

	public interface IEntityMappingConfiguration
	{
		#region Methods

		void Map(ModelBuilder b);

		#endregion
	}
}