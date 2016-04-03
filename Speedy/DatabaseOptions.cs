namespace Speedy
{
	/// <summary>
	/// Represents options for a Speedy database.
	/// </summary>
	public class DatabaseOptions
	{
		#region Properties

		/// <summary>
		/// Gets or sets the flag to manage the CreatedOn and optional ModifiedOn properties.
		/// </summary>
		public bool MaintainDates { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the defaults that all base database clases will use.
		/// </summary>
		/// <returns> The default values databases use. </returns>
		public static DatabaseOptions GetDefaults()
		{
			return new DatabaseOptions
			{
				MaintainDates = true
			};
		}

		#endregion
	}
}