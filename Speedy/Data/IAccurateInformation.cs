#region References

using Speedy.Data.Location;

#endregion

namespace Speedy.Data;

/// <summary>
/// Represents information that also contain accuracy value.
/// </summary>
public interface IAccurateInformation : IInformation
{
	#region Properties

	/// <summary>
	/// The accuracy of the information.
	/// </summary>
	double Accuracy { get; set; }

	/// <summary>
	/// The reference system for accuracy.
	/// </summary>
	AccuracyReferenceType AccuracyReference { get; set; }

	/// <summary>
	/// Specifies if the Accuracy value is valid.
	/// </summary>
	bool HasAccuracy { get; }

	#endregion
}