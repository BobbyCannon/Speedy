namespace Speedy.Validation
{
	/// <summary>
	/// Represents a validation for an object.
	/// </summary>
	public interface IValidation
	{
		#region Properties

		/// <summary>
		/// The message for failed validation.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The name for failed validation.
		/// </summary>
		public string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Tries to validate
		/// </summary>
		/// <returns> Returns true if the validation passes otherwise false. </returns>
		public bool TryValidate(object value);

		#endregion
	}
}