#region References

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Speedy.Sync;

#endregion

namespace Speedy.Exceptions
{
	/// <summary>
	/// Represents a sync issue exception.
	/// </summary>
	[Serializable]
	public class SyncIssueException : SpeedyException
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public SyncIssueException()
		{
			Issues = Array.Empty<SyncIssue>();
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public SyncIssueException(string message, IEnumerable<SyncIssue> issues) : base(message)
		{
			Issues = issues;
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public SyncIssueException(string message, Exception inner, IEnumerable<SyncIssue> issues) : base(message, inner)
		{
			Issues = issues;
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		protected SyncIssueException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the sync issues for this exception.
		/// </summary>
		public IEnumerable<SyncIssue> Issues { get; }

		#endregion
	}
}