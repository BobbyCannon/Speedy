namespace Speedy
{
	/// <summary>
	/// Represents options for a Speedy database.
	/// </summary>
	public class DatabaseOptions
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database options class.
		/// </summary>
		public DatabaseOptions()
		{
			MaintainDates = true;
			MaintainSyncId = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the flag to manage the CreatedOn and optional ModifiedOn properties.
		/// </summary>
		public bool MaintainDates { get; set; }

		/// <summary>
		/// Gets or sets the flag to manage the sync ID for sync entities.
		/// </summary>
		public bool MaintainSyncId { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the defaults that all base database classes will use.
		/// </summary>
		/// <returns> The default values databases use. </returns>
		public static DatabaseOptions GetDefaults()
		{
			return new DatabaseOptions();
		}

		#endregion
	}
}