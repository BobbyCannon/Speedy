#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Exceptions
{
	/// <summary>
	/// The base exception for the Speedy framework.
	/// </summary>
	public class SpeedyException : Exception
	{
		#region Constants

		/// <summary>
		/// Represents message for key not found.
		/// </summary>
		public const string KeyNotFound = "Could not find the entry with the key.";

		/// <summary>
		/// Represents message for repository not found.
		/// </summary>
		public const string RepositoryNotFound = "The repository was not found.";

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		public SpeedyException()
		{
		}

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		public SpeedyException(string message) : base(message)
		{
		}

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		public SpeedyException(string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Instantiates an instance of the speedy exception.
		/// </summary>
		public SpeedyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion
	}
}