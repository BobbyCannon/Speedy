#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Exceptions;

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
	public SyncIssueException() : this(SyncIssueType.Unknown, string.Empty)
	{
	}

	/// <summary>
	/// Instantiates an instance of the exception.
	/// </summary>
	public SyncIssueException(SyncIssueType type, string message, params SyncIssue[] issues)
		: this(type, message, null, issues)
	{
	}

	/// <summary>
	/// Instantiates an instance of the exception.
	/// </summary>
	public SyncIssueException(SyncIssueType type, string message, Exception inner, params SyncIssue[] issues)
		: base(message, inner)
	{
		IssueType = type;
		Issues = issues ?? Array.Empty<SyncIssue>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the child sync issues for this exception.
	/// </summary>
	public IEnumerable<SyncIssue> Issues { get; set; }

	/// <summary>
	/// Gets the type of the issue.
	/// </summary>
	public SyncIssueType IssueType { get; set; }

	#endregion
}