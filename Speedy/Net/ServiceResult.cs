#region References

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Represents the results for a service request.
	/// </summary>
	/// <typeparam name="T"> The type of the items in the results collection. </typeparam>
	public class ServiceResult<T> : Bindable
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of a request result.
		/// </summary>
		public ServiceResult() : this(new T[0])
		{
		}

		/// <summary>
		/// Instantiates an instance of a request result.
		/// </summary>
		public ServiceResult(params T[] collection) : this(collection.ToList())
		{
		}

		/// <summary>
		/// Instantiates an instance of a request result.
		/// </summary>
		public ServiceResult(IEnumerable<T> collection)
		{
			Collection = collection.ToList();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The results for a service request.
		/// </summary>
		public IList<T> Collection { get; set; }

		/// <summary>
		/// The value to determine if the request has more pages.
		/// </summary>
		public bool HasMore => Collection.Count > 0 && Skipped + Collection.Count < TotalCount;

		/// <summary>
		/// The number of items that was skipped to start this result.
		/// </summary>
		public int Skipped { get; set; }

		/// <summary>
		/// The total count of items for the request.
		/// </summary>
		public int TotalCount { get; set; }

		#endregion
	}
}