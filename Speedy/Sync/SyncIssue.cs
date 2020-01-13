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
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets the type name of the object.
		/// </summary>
		public string TypeName { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public SyncIssue Convert(string newTypeName)
		{
			var destination = new SyncIssue();

			// Handle all one to one properties (same name & type) and all sync entity base properties.
			// This will override any exclusions. Meaning this entity will copy all possible properties.
			destination.UpdateWith(this, true);
			destination.TypeName = newTypeName;

			return destination;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{IssueType}:{TypeName} - {Message}";
		}

		#endregion
	}
}