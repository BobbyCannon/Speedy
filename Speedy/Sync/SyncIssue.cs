#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents as issue that occurred during sync.
	/// </summary>
	public class SyncIssue
	{
		#region Properties

		/// <summary>
		/// The ID of the sync item.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The type of issue. Example like CannotDelete due to relationship dependencies.
		/// </summary>
		public SyncIssueType IssueType { get; set; }

		/// <summary>
		/// Get the description of the issue.
		/// </summary>
		public string Message { get; internal set; }

		/// <summary>
		/// Gets or sets the type name of the object.
		/// </summary>
		public string TypeName { get; set; }

		#endregion
	}
}