namespace Speedy.Validation
{
	/// <summary>
	/// Extensions for member validators.
	/// </summary>
	public static class MemberValidatorExtensions
	{
		#region Methods

		/// <summary>
		/// Validates that a member is not null or whitespace.
		/// </summary>
		/// <param name="validator"> The validator to be extended. </param>
		/// <returns> True if the value is and within the provided range. </returns>
		public static MemberValidator<string> IsNotNullOrWhitespace(this MemberValidator<string> validator)
		{
			return IsNotNullOrWhitespace(validator, $"{validator.Info.Name} is null or whitespace.");
		}

		/// <summary>
		/// Validates that a member is not null or whitespace.
		/// </summary>
		/// <param name="validator"> The validator to be extended. </param>
		/// <param name="message"> The message for failed validation. </param>
		/// <returns> True if the value is and within the provided range. </returns>
		public static MemberValidator<string> IsNotNullOrWhitespace(this MemberValidator<string> validator, string message)
		{
			var validation = new Validation<string>(validator, message, x => !string.IsNullOrWhiteSpace(x));
			validator.Validations.Add(validation);
			return validator;
		}

		#endregion
	}
}