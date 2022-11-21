#region References

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Speedy.Extensions;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.Protocols.Csv;

/// <summary>
/// Converts a list of items to CSV format.
/// </summary>
public class CsvWriter<T> : CsvWriter
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the CSV writer.
	/// </summary>
	/// <param name="columnSeparator">
	/// The string used to separate columns in the output.
	/// By default this is a comma so that the generated output is a CSV file.
	/// </param>
	/// <param name="includeHeaderRow">
	/// Whether to include the header row with the columns names in the export
	/// </param>
	public CsvWriter(string columnSeparator = ",", bool includeHeaderRow = true)
		: base(typeof(T), columnSeparator, includeHeaderRow)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Add a list of items as rows.
	/// </summary>
	public void Add(params T[] items)
	{
		if (!items.Any())
		{
			return;
		}

		foreach (var obj in items)
		{
			var row = new Dictionary<string, object>();

			foreach (var value in Properties)
			{
				row.Add(value.Name, value.GetValue(obj, null));
			}

			Rows.Add(row);
		}
	}

	#endregion
}

/// <summary>
/// Converts a list of items to CSV format.
/// </summary>
public class CsvWriter
{
	#region Fields

	private readonly Type _type;

	/// <summary>
	/// The string used to separate columns in the output.
	/// </summary>
	private readonly string _columnSeparator;

	/// <summary>
	/// Whether to include the header row with column names.
	/// </summary>
	private readonly bool _includeHeaderRow;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the CSV writer.
	/// </summary>
	/// <param name="type"> The type this writer is for </param>
	/// <param name="columnSeparator">
	/// The string used to separate columns in the output.
	/// By default this is a comma so that the generated output is a CSV file.
	/// </param>
	/// <param name="includeHeaderRow">
	/// Whether to include the header row with the columns names in the export
	/// </param>
	public CsvWriter(Type type, string columnSeparator = ",", bool includeHeaderRow = true)
	{
		_type = type;
		_columnSeparator = columnSeparator;
		_includeHeaderRow = includeHeaderRow;

		Properties = type.GetCachedProperties().OrderBy(x => x.Name).ToArray();
		Rows = new List<Dictionary<string, object>>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The list of fields (headers).
	/// </summary>
	protected PropertyInfo[] Properties { get; }

	/// <summary>
	/// The list of rows.
	/// </summary>
	protected List<Dictionary<string, object>> Rows { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Converts a value to how it should output in a csv file
	/// If it has a comma, it needs surrounding with double quotes
	/// Eg Sydney, Australia -> "Sydney, Australia"
	/// Also if it contains any double quotes ("), then they need to be replaced with quad quotes[sic] ("")
	/// Eg "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
	/// </summary>
	/// <param name="value"> The value to escape. </param>
	/// <param name="columnSeparator">
	/// The string used to separate columns in the output.
	/// By default this is a comma so that the generated output is a CSV document.
	/// </param>
	public static string EscapeValue(object value, string columnSeparator = ",")
	{
		if (value == null)
		{
			return "";
		}
		if (value is INullable && ((INullable) value).IsNull)
		{
			return "";
		}

		string output;
		switch (value)
		{
			case Enum eValue:
			{
				output = Convert.ToUInt64(eValue).ToString();
				break;
			}
			case DateTime dateTime:
			{
				output = dateTime.ToString(dateTime.TimeOfDay.TotalSeconds == 0 ? "yyyy-MM-dd" : "O");
				break;
			}
			case OscTimeTag oscTimeTag:
			{
				var dateTime = (DateTime) oscTimeTag;
				output = dateTime.ToString(dateTime.TimeOfDay.TotalSeconds == 0 ? "yyyy-MM-dd" : "O");
				break;
			}
			default:
			{
				output = value.ToString().Trim();
				break;
			}
		}

		if (output.Length > 30000)
		{
			// cropping value for  Excel
			output = output.Substring(0, 30000);
		}

		if (output.Contains(columnSeparator)
			|| output.Contains("\"")
			|| output.Contains("\n")
			|| output.Contains("\r"))
		{
			output = $"\"{output.Replace("\"", "\"\"")}\"";
		}

		return output;
	}

	/// <summary>
	/// Exports as raw bytes.
	/// </summary>
	public byte[] ExportToBytes()
	{
		using (var ms = new MemoryStream())
		{
			var preamble = Encoding.UTF8.GetPreamble();
			ms.Write(preamble, 0, preamble.Length);

			using (var sw = new StreamWriter(ms, Encoding.UTF8))
			{
				foreach (var line in ExportToLines())
				{
					var i = 0;
					foreach (var value in line)
					{
						sw.Write(value);

						if (++i != Properties.Length)
						{
							sw.Write(_columnSeparator);
						}
					}
					sw.Write("\r\n");
				}

				sw.Flush(); //otherwise we're risking empty stream
			}
			return ms.ToArray();
		}
	}

	/// <summary>
	/// Exports to a file
	/// </summary>
	public void ExportToFile(string path)
	{
		File.WriteAllBytes(path, ExportToBytes());
	}

	/// <summary>
	/// Writes a list of items to CSV format.
	/// </summary>
	/// <typeparam name="T"> The type this writer is for. </typeparam>
	/// <param name="items"> The items to be exported. </param>
	/// <param name="columnSeparator">
	/// The string used to separate columns in the output.
	/// By default this is a comma so that the generated output is a CSV file.
	/// </param>
	/// <param name="includeHeaderRow">
	/// Whether to include the header row with the columns names in the export
	/// </param>
	public static string Write<T>(IEnumerable<T> items, string columnSeparator = ",", bool includeHeaderRow = true)
	{
		var array = items.ToArray();
		var first = array.FirstOrDefault();
		if (first == null)
		{
			return null;
		}

		var type = first.GetType();
		var writer = new CsvWriter(type, columnSeparator, includeHeaderRow);
		writer.Add(array);
		return writer.Write();
	}

	/// <summary>
	/// Output all rows as a CSV string.
	/// </summary>
	public string Write()
	{
		var sb = new StringBuilder();

		foreach (var line in ExportToLines())
		{
			foreach (var value in line)
			{
				sb.Append(value);
				sb.Append(_columnSeparator);
			}

			sb.Length -= _columnSeparator.Length;
			sb.Append("\r\n");
		}

		// Remove the last new line
		sb.Length -= 2;

		return sb.ToString();
	}

	private void Add<T>(params T[] items)
	{
		if (!items.Any())
		{
			return;
		}

		foreach (var obj in items)
		{
			if (obj.GetType() != _type)
			{
				continue;
			}

			var row = new Dictionary<string, object>();

			foreach (var value in Properties)
			{
				row.Add(value.Name, value.GetValue(obj));
			}

			Rows.Add(row);
		}
	}

	/// <summary>
	/// Outputs all rows as a CSV, returning one "line" at a time
	/// Where "line" is a IEnumerable of string values
	/// </summary>
	private IEnumerable<IEnumerable<string>> ExportToLines()
	{
		// The header of property names.
		if (_includeHeaderRow)
		{
			yield return Properties.Select(f => EscapeValue(f.Name, _columnSeparator));
		}

		// The rows of data.
		foreach (var row in Rows)
		{
			yield return Properties
				.Select(field => row.TryGetValue(field.Name, out var value)
					? EscapeValue(value)
					: ""
				);
		}
	}

	#endregion
}