#region References

using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Exceptions
{
	/// <summary>
	/// Represents a web client exception.
	/// </summary>
	[Serializable]
	public class WebClientException : SpeedyException
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public WebClientException()
		{
			Code = HttpStatusCode.OK;
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public WebClientException(HttpResponseMessage result)
			: this(result.StatusCode, result.Content.ReadAsStringAsync().Result)
		{
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public WebClientException(HttpStatusCode code, string message) : base(message)
		{
			Code = code;
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public WebClientException(HttpStatusCode code, string message, Exception inner) : base(message, inner)
		{
			Code = code;
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		protected WebClientException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Code = HttpStatusCode.OK;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The HTTP status code for this exception.
		/// </summary>
		public HttpStatusCode Code { get; }

		#endregion
	}
}