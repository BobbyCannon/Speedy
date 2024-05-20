#region References

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Speedy.Internal;

#endregion

namespace Speedy.Runtime;

/// <summary>
/// Provides unique device identifiers.
/// </summary>
public class DeviceId
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DeviceId" /> class.
	/// </summary>
	public DeviceId()
	{
		Formatter = new HashDeviceIdFormatter(new ByteArrayHasher(SHA256.Create), new Base32ByteArrayEncoder(Base32ByteArrayEncoder.CrockfordAlphabet));
		Components = new Dictionary<string, IDeviceIdComponent>(StringComparer.OrdinalIgnoreCase);
	}

	#endregion

	#region Properties

	/// <summary>
	/// A dictionary containing the components that will make up the device identifier.
	/// </summary>
	public IDictionary<string, IDeviceIdComponent> Components { get; }

	/// <summary>
	/// Gets or sets the formatter to use.
	/// </summary>
	internal IDeviceIdFormatter Formatter { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Adds a component to the device identifier.
	/// If a component with the specified name already exists, it will be replaced with this newly added component.
	/// </summary>
	/// <param name="name"> The component name. </param>
	/// <param name="component"> The component to add. </param>
	/// <returns> The builder instance. </returns>
	public DeviceId AddComponent(string name, IDeviceIdComponent component)
	{
		Components[name] = component;
		return this;
	}

	/// <summary>
	/// Adds the machine name to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public DeviceId AddMachineName()
	{
		return AddComponent("MachineName", new DeviceIdComponent(Environment.MachineName));
	}

	/// <summary>
	/// Adds the operating system version to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public DeviceId AddOsVersion()
	{
		return AddComponent("OSVersion", new DeviceIdComponent(Environment.OSVersion));
	}

	/// <summary>
	/// Adds the current username to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public DeviceId AddUserName()
	{
		return AddComponent("UserName", new DeviceIdComponent(Environment.UserName));
	}

	/// <summary>
	/// Returns a string representation of the device identifier.
	/// </summary>
	/// <returns> A string representation of the device identifier. </returns>
	public override string ToString()
	{
		if (Formatter == null)
		{
			throw new InvalidOperationException($"The {nameof(Formatter)} property must not be null in order for {nameof(ToString)} to be called.");
		}

		return Formatter.GetDeviceId(Components);
	}

	#endregion
}