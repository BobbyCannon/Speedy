#region References

using System.Collections.Generic;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea;

public class NavigationStatus
{
	#region Fields

	private readonly Dictionary<char, string> _navigationalStatuses;

	#endregion

	#region Constructors

	public NavigationStatus(string status)
	{
		_navigationalStatuses = new Dictionary<char, string>
		{
			{ 'C', "Caution" },
			{ 'S', "Safe" },
			{ 'U', "Unsafe" },
			{ 'V', "Void" }
		};

		if (!string.IsNullOrEmpty(status) && _navigationalStatuses.ContainsKey(status[0]))
		{
			Status = status[0];
			StatusName = _navigationalStatuses[Status];
		}
		else
		{
			Status = ' ';
			StatusName = string.Empty;
		}
	}

	#endregion

	#region Properties

	public char Status { get; }

	public string StatusName { get; }

	#endregion

	#region Methods

	public bool IsSet()
	{
		return _navigationalStatuses.ContainsKey(Status);
	}

	public override string ToString()
	{
		return Status.ToString();
	}

	#endregion
}