#region References

using System.ComponentModel.DataAnnotations;

#endregion

namespace Speedy.Validation
{
	/// <summary>
	/// Represents validation exception types
	/// </summary>
	public enum ValidationExceptionType
	{
		/// <summary>
		/// Represents an unknown error
		/// </summary>
		[Display(Name = "Unknown", Description = "Unknown validation error for {0}.")]
		Unknown = 0,

		/// <summary>
		/// Represents an AreEqual error
		/// </summary>
		[Display(Name = "Are Equal", Description = "{0} does not equal the required value.")]
		AreEqual = 1,

		/// <summary>
		/// Represents an NotEqual error
		/// </summary>
		[Display(Name = "Not Equal", Description = "{0} does not equal the required value.")]
		NotEqual = 2,

		/// <summary>
		/// Represents an IsNull error
		/// </summary>
		[Display(Name = "Is Null", Description = "{0} is not null but is required to be.")]
		IsNull = 3,

		/// <summary>
		/// Represents an IsNotNull error
		/// </summary>
		[Display(Name = "Is Not Null", Description = "{0} is null but a value is required.")]
		IsNotNull = 4,

		/// <summary>
		/// Represents an MinMaxRange error
		/// </summary>
		[Display(Name = "Min Max Range", Description = "{0} is not within the required range values.")]
		MinMaxRange = 5,

		/// <summary>
		/// Represents an EnumRange error
		/// </summary>
		[Display(Name = "Enum Range", Description = "{0} does not contain a valid enum value.")]
		EnumRange = 6,

		/// <summary>
		/// Represents an IsRequired error
		/// </summary>
		[Display(Name = "Is Required", Description = "{0} is required but was not provided.")]
		IsRequired = 7,

		/// <summary>
		/// Represents an IsOptional error
		/// </summary>
		[Display(Name = "Is Optional", Description = "{0} is optional but was not provided.")]
		IsOptional = 8,

		/// <summary>
		/// Represents an NoLessThan error
		/// </summary>
		[Display(Name = "No Less Than", Description = "{0} is less than the required minimum value.")]
		NoLessThan = 9,

		/// <summary>
		/// Represents an NoMoreThan error
		/// </summary>
		[Display(Name = "No More Than", Description = "{0} is less than the required maximum value.")]
		NoMoreThan = 10,

		/// <summary>
		/// Represents an IsTrue error
		/// </summary>
		[Display(Name = "Is True", Description = "{0} is not true but is required to be.")]
		IsTrue = 11,

		/// <summary>
		/// Represents an IsFalse error
		/// </summary>
		[Display(Name = "Is False", Description = "{0} is not false but is required to be.")]
		IsFalse = 12
	}
}