#region References

using System;

#endregion

namespace Speedy.Runtime;

/// <summary>
/// An implementation of <see cref="IDeviceIdComponent" /> that uses either a specified value
/// or the result of a specified function as its component value.
/// </summary>
public class DeviceIdComponent : IDeviceIdComponent
{
	#region Fields

	/// <summary>
	/// A function that returns the component value.
	/// </summary>
	private readonly Func<string> _valueFactory;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DeviceIdComponent" /> class.
	/// </summary>
	/// <param name="value"> The component value. </param>
	public DeviceIdComponent(string value)
		: this(() => value)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeviceIdComponent" /> class.
	/// </summary>
	/// <param name="valueFactory"> A function that returns the component value. </param>
	public DeviceIdComponent(Func<string> valueFactory)
	{
		_valueFactory = valueFactory;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeviceIdComponent" /> class.
	/// </summary>
	/// <param name="value"> A function that returns the component value. </param>
	public DeviceIdComponent(object value) : this(() => value?.ToString() ?? string.Empty)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the component value.
	/// </summary>
	/// <returns> The component value. </returns>
	public string GetValue()
	{
		return _valueFactory.Invoke();
	}

	#endregion
}

/// <summary>
/// Represents a component that forms part of a device identifier.
/// </summary>
public interface IDeviceIdComponent
{
	#region Methods

	/// <summary>
	/// Gets the component value.
	/// </summary>
	/// <returns> The component value. </returns>
	string GetValue();

	#endregion
}