using System.Linq;

namespace Speedy.Configuration
{
	/// <summary>
	/// The interface for the property configuration.
	/// </summary>
	public interface IPropertyConfiguration
	{
		#region Methods

		/// <summary>
		/// Checks to see if this configuration is for the provided entity.
		/// </summary>
		/// <param name="entity"> The entity to test against. </param>
		/// <returns> True if this configuration is for the entity and false if otherwise. </returns>
		bool IsMappingFor(object entity);

		/// <summary>
		/// Validates the entity using this configuration.
		/// </summary>
		/// <param name="entity"> The entity to validate. </param>
		/// <param name="repository"> The entity repository. </param>
		void Validate(object entity, IQueryable repository);

		#endregion
	}
}