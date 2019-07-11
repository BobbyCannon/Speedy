#region References

using System;
using Speedy.Storage;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents options for a Speedy database.
	/// </summary>
	public class DatabaseOptions : IUpdatable<DatabaseOptions>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database options class.
		/// </summary>
		public DatabaseOptions()
		{
			DetectSyncableRepositories = true;
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
		/// Gets or sets the flag to automatically detect syncable repositories.
		/// </summary>
		public bool DetectSyncableRepositories { get; set; }

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
		public void Update(DatabaseOptions value)
		{
			if (value == null)
			{
				return;
			}

			DetectSyncableRepositories = value.DetectSyncableRepositories;
			DisableEntityValidations = value.DetectSyncableRepositories;
			MaintainCreatedOn = value.MaintainCreatedOn;
			MaintainModifiedOn = value.MaintainModifiedOn;
			MaintainSyncId = value.MaintainSyncId;
			PermanentSyncEntityDeletions = value.PermanentSyncEntityDeletions;
			SyncOrder = (string[]) value.SyncOrder.Clone();
			Timeout = value.Timeout;
			UnmaintainEntities = (Type[]) value.UnmaintainEntities.Clone();
		}

		/// <inheritdoc />
		public void Update(object value)
		{
			Update(value as DatabaseOptions);
		}

		#endregion
	}
}