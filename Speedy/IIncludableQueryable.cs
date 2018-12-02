#region References

using System;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Supports queryable Include/ThenInclude chaining operators.
	/// </summary>
	/// <typeparam name="T"> The entity type. </typeparam>
	/// <typeparam name="T2"> The type of the related entity to be included. </typeparam>
	public interface IIncludableQueryable<out T, T2> : IQueryable<T> where T : class
	{
		#region Methods

		/// <summary>
		/// Process the ThenInclude on an entity collection.
		/// </summary>
		/// <typeparam name="TPreviousProperty"> The previous property type of the related entity to be included. </typeparam>
		/// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
		/// <param name="include"> A lambda expression representing the navigation property to be included (<c> t =&gt; t.Property1 </c>). </param>
		/// <returns> A new query with the related data included. </returns>
		IIncludableQueryable<T, TProperty> ProcessCollectionThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include);

		/// <summary>
		/// Specifies additional related data to be further included based on a related type that was just included.
		/// </summary>
		/// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
		/// <param name="include"> A lambda expression representing the navigation property to be included (<c> t =&gt; t.Property1 </c>). </param>
		/// <returns> A new query with the related data included. </returns>
		IIncludableQueryable<T, TProperty> ThenInclude<TProperty>(Expression<Func<T2, TProperty>> include);

		#endregion
	}
}