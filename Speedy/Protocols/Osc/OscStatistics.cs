#region References

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public class OscStatistics : Dictionary<string, OscStatisticValue>, IEnumerable<OscStatisticValue>
{
	#region Constructors

	public OscStatistics(params OscStatisticValue[] values)
	{
		foreach (var v in values)
		{
			Add(v);
		}
	}

	#endregion

	#region Methods

	public OscStatisticValue Add(OscStatisticValue value)
	{
		if (ContainsKey(value.Name))
		{
			this[value.Name] = value;
		}
		else
		{
			Add(value.Name, value);
		}
		return value;
	}

	public void InvokePropertyChanged()
	{
		foreach (var value in Values)
		{
			value.InvokePropertyChanged();
		}
	}

	/// <summary>
	/// Reset the values for all statistic values in this set.
	/// </summary>
	public void Reset()
	{
		foreach (var value in Values)
		{
			value.Reset();
		}
	}

	/// <summary>
	/// Sets the update interval for all statistic values in this set.
	/// </summary>
	/// <param name="interval"> </param>
	public void SetAllUpdateIntervals(TimeSpan interval)
	{
		foreach (var value in Values)
		{
			value.UpdateInterval = interval;
		}
	}

	public void Update()
	{
		foreach (var value in Values)
		{
			value.UpdateRate();
		}
	}

	IEnumerator<OscStatisticValue> IEnumerable<OscStatisticValue>.GetEnumerator()
	{
		return Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}