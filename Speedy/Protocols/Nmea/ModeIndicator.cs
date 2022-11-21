#region References

using System.Collections.Generic;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea;

/// <summary>
/// Represents a mode indicator.
/// </summary>
public class ModeIndicator
{
	#region Fields

	private readonly Dictionary<char, string> _modeIndicators;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the mode indicator.
	/// </summary>
	/// <param name="modeIndicator"> The mode indicator in string format. </param>
	public ModeIndicator(string modeIndicator)
	{
		// Mode Indicator:
		_modeIndicators = new Dictionary<char, string>
		{
			{ 'A', "Autonomous" },
			{ 'D', "Differential" },
			{ 'E', "Estimated" },
			{ 'F', "Float RTK" },
			{ 'M', "Manual" },
			{ 'N', "No Fix" },
			{ 'P', "Precise" },
			{ 'R', "Real Time Kinematic" },
			{ 'S', "Simulator" }
		};

		if (!string.IsNullOrEmpty(modeIndicator) && _modeIndicators.ContainsKey(modeIndicator[0]))
		{
			Mode = modeIndicator[0];
			ModeName = _modeIndicators[Mode];
		}
		else
		{
			Mode = ' ';
			ModeName = string.Empty;
		}
	}

	#endregion

	#region Properties

	/// <summary>
	/// The mode character.
	/// </summary>
	public char Mode { get; }

	/// <summary>
	/// The name of the mode.
	/// </summary>
	public string ModeName { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Determines if the mode is set.
	/// </summary>
	/// <returns> </returns>
	public bool IsSet()
	{
		return _modeIndicators.ContainsKey(Mode);
	}

	/// <summary>
	/// Determines if the mode indicator is valid.
	/// </summary>
	/// <returns> </returns>
	public bool IsValid()
	{
		return Mode != 'N';
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return Mode.ToString();
	}

	#endregion
}