#region References

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#endregion

namespace Speedy.EntityFramework;

/// <summary>
/// Extensions to assist in upgrading of old ef to new EF without tons of code changes.
/// </summary>
public static class EntityFrameworkExtension
{
	#region Methods

	/// <summary>
	/// Configures the name of the index in the database when targeting a relational database.
	/// </summary>
	/// <param name="builder"> The builder for the index being configured. </param>
	/// <param name="name"> The name of the index. </param>
	/// <returns> A builder to further configure the index. </returns>
	public static IndexBuilder HasIndexName(this IndexBuilder builder, string name)
	{
		#if (NET48_OR_GREATER || NETSTANDARD2_0)
		return builder.HasName(name);
		#else
		return builder.HasDatabaseName(name);
		#endif
	}

	/// <summary>
	/// Configures the name of the index in the database when targeting a relational database.
	/// </summary>
	/// <typeparam name="T"> The entity type being configured. </typeparam>
	/// <param name="builder"> The builder for the index being configured. </param>
	/// <param name="name"> The name of the index. </param>
	/// <returns> A builder to further configure the index. </returns>
	public static IndexBuilder<T> HasIndexName<T>(this IndexBuilder<T> builder, string name)
	{
		#if (NET48_OR_GREATER || NETSTANDARD2_0)
		return builder.HasName(name);
		#else
		return builder.HasDatabaseName(name);
		#endif
	}

	#endregion
}