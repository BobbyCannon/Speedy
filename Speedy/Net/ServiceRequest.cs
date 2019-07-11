#region References

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Represents a service request containing a collection.
	/// </summary>
	/// <typeparam name="T"> The type of the item collection. </typeparam>
	public class ServiceRequest<T> : ServiceRequest
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of a service request.
		/// </summary>
		public ServiceRequest() : this(new T[0])
		{
		}

		/// <summary>
		/// Instantiates an instance of a service request.
		/// </summary>
		public ServiceRequest(params T[] collection) : this(collection.ToList())
		{
		}

		/// <summary>
		/// Instantiates an instance of a service request.
		/// </summary>
		public ServiceRequest(IEnumerable<T> collection)
		{
			Collection = collection.ToList();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The collection of items to include in the request.
		/// </summary>
		public IList<T> Collection { get; set; }

		#endregion
	}

	/// <summary>
	/// Represents a service request.
	/// </summary>
	public class ServiceRequest : Bindable
	{
		#region Properties

		/// <summary>
		/// The optional collection of filter values.
		/// </summary>
		public IDictionary<string, string> Filters { get; set; }

		/// <summary>
		/// The values to be include in the results. Defaults to an empty collection.
		/// </summary>
		public IList<string> Including { get; set; }

		/// <summary>
		/// The optional collection of request options.
		/// </summary>
		public IDictionary<string, string> Options { get; set; }

		/// <summary>
		/// The number of items to skip.
		/// </summary>
		public int Skip { get; set; }

		/// <summary>
		/// The number of items requested.
		/// </summary>
		public int Take { get; set; }

		#endregion
	}
}