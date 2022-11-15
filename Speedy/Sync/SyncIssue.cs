#region References

using System;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
    /// <summary>
    /// Represents as issue that occurred during sync.
    /// </summary>
    public class SyncIssue : Bindable, IUpdatable<SyncIssue>
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
			destination.UpdateWith(this);
			destination.TypeName = newTypeName;

			return destination;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{IssueType}:{TypeName} - {Message}";
		}

		/// <inheritdoc />
		public virtual bool ShouldUpdate(SyncIssue update)
		{
			return true;
		}

		/// <summary>
		/// Update the SyncStatistics with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public bool UpdateWith(SyncIssue update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return false;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				Id = update.Id;
				IssueType = update.IssueType;
				Message = update.Message;
				TypeName = update.TypeName;
			}
			else
			{
				this.IfThen(x => !exclusions.Contains(nameof(Id)), x => x.Id = update.Id);
				this.IfThen(x => !exclusions.Contains(nameof(IssueType)), x => x.IssueType = update.IssueType);
				this.IfThen(x => !exclusions.Contains(nameof(Message)), x => x.Message = update.Message);
				this.IfThen(x => !exclusions.Contains(nameof(TypeName)), x => x.TypeName = update.TypeName);
			}

			return true;
		}

		/// <inheritdoc />
		public override bool UpdateWith(object update, params string[] exclusions)
		{
			return update switch
			{
				SyncIssue options => UpdateWith(options, exclusions),
				_ => base.UpdateWith(update, exclusions)
			};
		}

		#endregion
	}
}