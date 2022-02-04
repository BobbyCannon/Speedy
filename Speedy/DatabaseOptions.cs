#region References

using System;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents options for a Speedy database.
	/// </summary>
	public class DatabaseOptions : CloneableBindable<DatabaseOptions>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database options class.
		/// </summary>
		public DatabaseOptions()
		{
			DisableEntityValidations = false;
			MaintainCreatedOn = true;
			MaintainModifiedOn = true;
			MaintainSyncId = true;
			PermanentSyncEntityDeletions = false;
			SyncOrder = Array.Empty<string>();
			Timeout = TimeSpan.FromSeconds(30);
			UnmaintainedEntities = Array.Empty<Type>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the flag to disable entity validations.
		/// </summary>
		public bool DisableEntityValidations { get; set; }

		/// <summary>
		/// Gets or sets the flag to manage the optional CreatedOn property.
		/// </summary>
		public bool MaintainCreatedOn { get; set; }

		/// <summary>
		/// Gets or sets the flag to manage the optional ModifiedOn properties.
		/// </summary>
		public bool MaintainModifiedOn { get; set; }

		/// <summary>
		/// Gets or sets the flag to manage the sync ID for sync entities.
		/// </summary>
		public bool MaintainSyncId { get; set; }

		/// <summary>
		/// If true the sync entities will actually delete entities marked for deletion. Defaults to false where IsDeleted will be marked "true".
		/// </summary>
		/// todo: update saving of modified entities to ignore changes to deleted sync entities?
		public bool PermanentSyncEntityDeletions { get; set; }

		/// <summary>
		/// Gets or sets the sync order of the syncable repositories.
		/// </summary>
		public string[] SyncOrder { get; set; }

		/// <summary>
		/// Gets or sets the timeout for blocking calls.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Gets or sets the list of entities to ignore during maintenance updates.
		/// </summary>
		public Type[] UnmaintainedEntities { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override object DeepClone(int? maxDepth = null)
		{
			var response = new DatabaseOptions();
			response.UpdateWith(this);
			return response;
		}

		/// <summary>
		/// Update these database options.
		/// </summary>
		/// <param name="update"> </param>
		/// <param name="exclusions"> </param>
		public override void UpdateWith(DatabaseOptions update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				DisableEntityValidations = update.DisableEntityValidations;
				MaintainCreatedOn = update.MaintainCreatedOn;
				MaintainModifiedOn = update.MaintainModifiedOn;
				MaintainSyncId = update.MaintainSyncId;
				PermanentSyncEntityDeletions = update.PermanentSyncEntityDeletions;
				SyncOrder = (string[]) update.SyncOrder.Clone();
				Timeout = update.Timeout;
				UnmaintainedEntities = (Type[]) update.UnmaintainedEntities.Clone();
			}
			else
			{
				this.IfThen(x => !exclusions.Contains(nameof(DisableEntityValidations)), x => x.DisableEntityValidations = update.DisableEntityValidations);
				this.IfThen(x => !exclusions.Contains(nameof(MaintainCreatedOn)), x => x.MaintainCreatedOn = update.MaintainCreatedOn);
				this.IfThen(x => !exclusions.Contains(nameof(MaintainModifiedOn)), x => x.MaintainModifiedOn = update.MaintainModifiedOn);
				this.IfThen(x => !exclusions.Contains(nameof(MaintainSyncId)), x => x.MaintainSyncId = update.MaintainSyncId);
				this.IfThen(x => !exclusions.Contains(nameof(PermanentSyncEntityDeletions)), x => x.PermanentSyncEntityDeletions = update.PermanentSyncEntityDeletions);
				this.IfThen(x => !exclusions.Contains(nameof(SyncOrder)), x => x.SyncOrder = (string[]) update.SyncOrder.Clone());
				this.IfThen(x => !exclusions.Contains(nameof(Timeout)), x => x.Timeout = update.Timeout);
				this.IfThen(x => !exclusions.Contains(nameof(UnmaintainedEntities)), x => x.UnmaintainedEntities = (Type[]) update.UnmaintainedEntities.Clone());
			}
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case DatabaseOptions options:
				{
					UpdateWith(options, exclusions);
					return;
				}
				default:
				{
					base.UpdateWith(update, exclusions);
					return;
				}
			}
		}

		#endregion
	}
}