#region References

using System;
using System.Linq.Expressions;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a repository filter
	/// </summary>
	/// <typeparam name="T"> The type for the filter. </typeparam>
	public class SyncRepositoryFilter<T> : SyncRepositoryFilter
	{
		#region Constructors

		/// <summary>
		/// Instantiates a repository filter.
		/// </summary>
		/// <param name="outgoingFilter"> The filter for the type for outgoing (GetChanges/GetCorrections). </param>
		/// <param name="incomingFilter"> The filter for the type for incoming (ApplyChanges/ApplyCorrections). </param>
		/// <param name="lookupFilter"> The filter for the type for looking up the entity (GetChanges/GetCorrections). </param>
		/// <param name="skipDeletedItemsOnInitialSync"> The option to skipped SyncEntity.IsDeleted on initial sync. </param>
		public SyncRepositoryFilter(Expression<Func<T, bool>> outgoingFilter = null, Expression<Func<T, bool>> incomingFilter = null, Func<T, Expression<Func<T, bool>>> lookupFilter = null, bool skipDeletedItemsOnInitialSync = true)
			: base(typeof(T).ToAssemblyName(), outgoingFilter, incomingFilter, lookupFilter, skipDeletedItemsOnInitialSync)
		{
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public override bool HasLookupFilter => LookupFilter != null;

		/// <summary>
		/// The incoming filter for the type.
		/// </summary>
		public Expression<Func<T, bool>> IncomingFilter => IncomingExpression as Expression<Func<T, bool>>;

		/// <summary>
		/// The look up expression for the type
		/// </summary>
		public Func<T, Expression<Func<T, bool>>> LookupFilter => LookupExpression as Func<T, Expression<Func<T, bool>>>;

		/// <summary>
		/// The outgoing filter for the type.
		/// </summary>
		public Expression<Func<T, bool>> OutgoingFilter => OutgoingExpression as Expression<Func<T, bool>>;

		#endregion

		#region Methods

		/// <summary>
		/// A test to validate if an incoming entity should be filtered.
		/// </summary>
		/// <param name="entity"> The entity to be tested. </param>
		/// <returns> True if the entity matches the incoming filter or false if otherwise. </returns>
		public bool ShouldFilterEntity(T entity)
		{
			// Only filter if the entity does not pass the test, default to passed if no incoming filter provided.
			return !(IncomingFilter?.Compile().Invoke(entity) ?? true);
		}

		#endregion
	}

	/// <summary>
	/// Represents a repository filter
	/// </summary>
	public abstract class SyncRepositoryFilter
	{
		#region Constructors

		/// <summary>
		/// Instantiates a repository filter.
		/// </summary>
		/// <param name="type"> The type this filter is for. </param>
		/// <param name="outgoingFilter"> The outgoing filter for the type. </param>
		/// <param name="incomingFilter"> The incoming filter for the type. </param>
		/// <param name="lookupFilter"> The lookup filter for the type. </param>
		/// <param name="skipDeletedItemsOnInitialSync"> The option to skipped SyncEntity.IsDeleted on initial sync. </param>
		public SyncRepositoryFilter(string type, object outgoingFilter, object incomingFilter, object lookupFilter, bool skipDeletedItemsOnInitialSync)
		{
			RepositoryType = type;
			OutgoingExpression = outgoingFilter;
			IncomingExpression = incomingFilter;
			LookupExpression = lookupFilter;
			SkipDeletedItemsOnInitialSync = skipDeletedItemsOnInitialSync;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Returns true if incoming expression is not null otherwise false.
		/// </summary>
		public virtual bool HasIncomingFilter => IncomingExpression != null;

		/// <summary>
		/// Returns true if lookup expression is not null otherwise false.
		/// </summary>
		public virtual bool HasLookupFilter => LookupExpression != null;

		/// <summary>
		/// The incoming filter as a generic object.
		/// </summary>
		public object IncomingExpression { get; }

		/// <summary>
		/// The lookup filter as a generic object.
		/// </summary>
		public object LookupExpression { get; }

		/// <summary>
		/// The outgoing filter as a generic object.
		/// </summary>
		public object OutgoingExpression { get; }

		/// <summary>
		/// The type contained in the repository.
		/// </summary>
		public string RepositoryType { get; }

		/// <summary>
		/// The option to skipped SyncEntity.IsDeleted on initial sync.
		/// </summary>
		public bool SkipDeletedItemsOnInitialSync { get; }

		#endregion
	}
}