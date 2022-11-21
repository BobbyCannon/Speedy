namespace Speedy.Protocols.Csv;

/// <summary>
/// Options for the CSV parser.
/// </summary>
public class CsvParserOptions
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the CSV parser options.
	/// </summary>
	public CsvParserOptions()
	{
		Delimiter = ',';
		HasHeader = false;
		TrimFields = false;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The character delimiter for the CSV file.
	/// </summary>
	public char Delimiter { get; set; }

	/// <summary>
	/// An options to parse out a header from the CSV file.
	/// </summary>
	public bool HasHeader { get; set; }

	/// <summary>
	/// If true start/end spaces are excluded from field values (except values in quotes). True by default.
	/// </summary>
	public bool TrimFields { get; set; }

	#endregion
}