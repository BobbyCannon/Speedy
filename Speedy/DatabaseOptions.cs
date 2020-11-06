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
	public class DatabaseOptions : Bindable<DatabaseOptions>
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
			SyncOrder = new string[0];
			Timeout = TimeSpan.FromSeconds(30);
			UnmaintainEntities = new Type[0];
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
		public Type[] UnmaintainEntities { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override void UpdateWith(DatabaseOptions update, params string[] exclusions)
		{
			if (update == null)
			{
				return;
			}

			this.IfThen(x => !exclusions.Contains(nameof(DisableEntityValidations)), x => x.DisableEntityValidations = update.DisableEntityValidations);
			this.IfThen(x => !exclusions.Contains(nameof(MaintainCreatedOn)), x => x.MaintainCreatedOn = update.MaintainCreatedOn);
			this.IfThen(x => !exclusions.Contains(nameof(MaintainModifiedOn)), x => x.MaintainModifiedOn = update.MaintainModifiedOn);
			this.IfThen(x => !exclusions.Contains(nameof(MaintainSyncId)), x => x.MaintainSyncId = update.MaintainSyncId);
			this.IfThen(x => !exclusions.Contains(nameof(PermanentSyncEntityDeletions)), x => x.PermanentSyncEntityDeletions = update.PermanentSyncEntityDeletions);
			this.IfThen(x => !exclusions.Contains(nameof(SyncOrder)), x => x.SyncOrder = (string[]) update.SyncOrder.Clone());
			this.IfThen(x => !exclusions.Contains(nameof(Timeout)), x => x.Timeout = update.Timeout);
			this.IfThen(x => !exclusions.Contains(nameof(UnmaintainEntities)), x => x.UnmaintainEntities = (Type[]) update.UnmaintainEntities.Clone());
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			UpdateWith(update as DatabaseOptions, exclusions);
		}

		#endregion
	}
}