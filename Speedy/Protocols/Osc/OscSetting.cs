#region References

using System;
using Speedy.Extensions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public abstract class OscSetting : OscCommand
{
	#region Constructors

	protected OscSetting(string address, int version, bool canRead = true, bool canWrite = true) : base(address, version)
	{
		CanRead = canRead;
		CanWrite = canWrite;
	}

	#endregion

	#region Properties

	public bool CanRead { get; set; }

	public bool CanWrite { get; set; }

	#endregion

	#region Methods

	public void RequestRead()
	{
		OnReadRequested();
	}

	public void RequestWrite()
	{
		OnWriteRequested();
	}

	public virtual void Update(OscSetting setting)
	{
		if (setting.GetType() != GetType())
		{
			throw new ArgumentException("The provided setting is not the correct type.");
		}

		if (Update(setting.OscMessage))
		{
			return;
		}

		// Fall back to reflection since we do not know the object structure.
		var properties = setting.GetCachedProperties();

		foreach (var p in properties)
		{
			if (!p.CanRead || !p.CanWrite)
			{
				continue;
			}

			p.SetValue(this, p.GetValue(setting));
		}
	}

	public virtual bool Update(OscMessage message)
	{
		// Faster than reflection
		return Load(message);
	}

	protected virtual void OnReadRequested()
	{
		if (!CanRead)
		{
			return;
		}

		ReadRequested?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnWriteRequested()
	{
		if (!CanWrite)
		{
			return;
		}

		WriteRequested?.Invoke(this, EventArgs.Empty);
	}

	#endregion

	#region Events

	public event EventHandler ReadRequested;
	public event EventHandler WriteRequested;

	#endregion
}