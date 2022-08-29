namespace Speedy.Validation
{
	internal class FailedValidation : IValidation
	{
		#region Constructors

		public FailedValidation(string name, string message)
		{
			Name = name;
			Message = message;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public string Message { get; }

		/// <inheritdoc />
		public string Name { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public bool TryValidate(object value)
		{
			return false;
		}

		#endregion
	}
}