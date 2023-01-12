#region References

using System;
using System.Diagnostics;
using System.Drawing;
using Speedy.Automation.Internal.Native;

#endregion

namespace Speedy.Automation.Desktop;

/// <summary>
/// Provided process details to safely work in x86 or x64 processes.
/// </summary>
public class SafeProcess : IDisposable
{
	#region Constructors

	internal SafeProcess(Process process)
	{
		Id = process.Id;
		Process = process;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the arguments that were provided when the process started.
	/// </summary>
	public string Arguments { get; set; }

	/// <summary>
	/// Gets the name of the file.
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	/// Gets the path to the file.
	/// </summary>
	public string FilePath { get; set; }

	/// <summary>
	/// Gets the handle to this process interaction.
	/// </summary>
	public nint Handle => Process.Handle;

	/// <summary>
	/// Gets a value indicating whether the associated process has been terminated.
	/// </summary>
	public bool HasExited => Process.HasExited;

	/// <summary>
	/// Gets the ID.
	/// </summary>
	public int Id { get; }

	/// <summary>
	/// Gets a flag indicating the process is 64 bit.
	/// </summary>
	public bool Is64Bit
	{
		get
		{
			if (!Environment.Is64BitOperatingSystem)
			{
				return false;
			}

			var result = NativeGeneral.IsWow64Process(Handle, out var isX86);
			if (result)
			{
				return !isX86;
			}

			return false;
		}
	}

	/// <summary>
	/// Gets a flag indicating the process is elevated.
	/// </summary>
	public bool IsElevated => NativeGeneral.IsElevated(Handle);

	/// <summary>
	/// Gets the main window handle.
	/// </summary>
	public nint MainWindowHandle
	{
		get
		{
			var count = 0;

			#if (NET7_0_OR_GREATER)
			var zero = nint.Zero;
			#else
			var zero = IntPtr.Zero;
			#endif

			try
			{
				while ((Process.MainWindowHandle == zero) && (count <= 2))
				{
					var id = Process.Id;
					Process?.Dispose();
					Process = Process.GetProcessById(id);
					count++;
				}
			}
			catch
			{
				return zero;
			}

			return Process.MainWindowHandle;
		}
	}

	/// <summary>
	/// Gets the name
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the details of the safe process.
	/// </summary>
	/// <seealso cref="System.Diagnostics.Process" />
	public Process Process { get; private set; }

	#endregion

	#region Methods

	/// <summary>
	/// Wait for the process to close if not then kill the process.
	/// </summary>
	/// <param name="timeout"> The timeout to wait for graceful close. If the timeout is reached then kill the process. The timeout is in milliseconds. </param>
	public void Close(int timeout = 0)
	{
		try
		{
			// See if the process has exited
			if ((Process == null) || Process.HasExited)
			{
				return;
			}

			// Ask the process to close gracefully and give it a chance to close.
			Process.Refresh();
			Process.CloseMainWindow();
			Process.WaitForExit(timeout);
		}
		catch
		{
			// Ignore any errors
		}
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Gets the process for this safe process.
	/// </summary>
	/// <returns> </returns>
	public Process GetProcess()
	{
		return Process;
	}

	/// <summary>
	/// First the main window location for the process.
	/// </summary>
	/// <returns> The location of the window. </returns>
	public Point GetWindowLocation()
	{
		var p = NativeGeneral.GetWindowPlacement(Process.MainWindowHandle);
		var location = p.rcNormalPosition.Location;

		if ((p.ShowState == 2) || (p.ShowState == 3))
		{
			NativeGeneral.GetWindowRect(Process.MainWindowHandle, out var windowsRect);
			location = new Point(windowsRect.Left + 8, windowsRect.Top + 8);
		}

		return location;
	}

	/// <summary>
	/// Gets the size of the main window for the process.
	/// </summary>
	/// <returns> The size of the main window. </returns>
	public Size GetWindowSize()
	{
		NativeGeneral.GetWindowRect(MainWindowHandle, out var data);
		return new Size(data.Right - data.Left, data.Bottom - data.Top);
	}

	/// <summary>
	/// Wait for the process to close if not then kill the process.
	/// </summary>
	/// <param name="timeout"> The timeout to wait for graceful close. If the timeout is reached then kill the process. The timeout is in milliseconds. </param>
	public void Kill(int timeout = 0)
	{
		try
		{
			// See if the process has already shutdown.
			if ((Process == null) || Process.HasExited)
			{
				return;
			}

			// OK, no more Mr. Nice Guy time to just kill the process.
			Process.Kill();
			Process.WaitForExit(timeout);
		}
		catch
		{
			// Ignore any errors
		}
	}

	/// <summary>
	/// Wait for the process to exit.
	/// </summary>
	/// <param name="timeout"> The timeout to wait for exit. The timeout is in milliseconds. </param>
	public void WaitForExit(int timeout = 10000)
	{
		try
		{
			// See if the process has exited
			if ((Process == null) || Process.HasExited)
			{
				return;
			}

			Process.WaitForExit(timeout);
		}
		catch
		{
			// Ignore any errors
		}
	}

	/// <summary>
	/// Wait for the process to go idle.
	/// </summary>
	/// <param name="milliseconds"> The time to wait for idle to occur. Time is in milliseconds. </param>
	public void WaitForInputIdle(int milliseconds)
	{
		Process?.WaitForInputIdle(milliseconds);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		Process?.Dispose();
		Process = null;
	}

	#endregion
}