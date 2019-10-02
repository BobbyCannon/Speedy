﻿#region References

using System;
using System.Linq.Expressions;

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
		public SyncRepositoryFilter(Expression<Func<T, bool>> outgoingFilter = null, Expression<Func<T, bool>> incomingFilter = null, Func<T, Expression<Func<T, bool>>> lookupFilter = null)
			: base(typeof(T).ToAssemblyName(), outgoingFilter, incomingFilter, lookupFilter)
		{
		}

		#endregion

		#region Properties

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
		/// A test to validate if an entity should be filtered.
		/// </summary>
		/// <param name="entity"> The entity to be tested. </param>
		/// <returns> True if the entity matches the filter or false if otherwise. </returns>
		public bool TestEntity(T entity)
		{
			return IncomingFilter?.Compile().Invoke(entity) ?? true;
		}

		#endregion
	}

	/// <summary>
	/// Represents a repository filter
	/// </summary>
	public class SyncRepositoryFilter
	{
		#region Constructors

		/// <summary>
		/// Instantiates a repository filter.
		/// </summary>
		/// <param name="type"> The type this filter is for. </param>
		/// <param name="outgoingFilter"> The outgoing filter for the type. </param>
		/// <param name="incomingFilter"> The incoming filter for the type. </param>
		/// <param name="lookupFilter"> The lookup filter for the type. </param>
		public SyncRepositoryFilter(string type, object outgoingFilter, object incomingFilter, object lookupFilter)
		{
			RepositoryType = type;
			OutgoingExpression = outgoingFilter;
			IncomingExpression = incomingFilter;
			LookupExpression = lookupFilter;
		}

		#endregion

		#region Properties

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

		#endregion
	}
}