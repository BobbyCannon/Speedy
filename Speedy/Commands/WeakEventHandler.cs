#region References

using System;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace Speedy.Commands;

/// <summary>
/// Represents as a weak event handler.
/// </summary>
/// <typeparam name="T"> The type of the event args. </typeparam>
[DebuggerNonUserCode]
public sealed class WeakEventHandler<T>
	where T : EventArgs
{
	#region Fields

	private readonly MethodInfo _method;
	private readonly WeakReference _targetReference;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the week event handler.
	/// </summary>
	/// <param name="callback"> The event handler callback. </param>
	public WeakEventHandler(EventHandler<T> callback)
	{
		_method = callback.Method;
		_targetReference = new WeakReference(callback.Target, true);
	}

	#endregion

	#region Methods

	/// <summary>
	/// The handler for the event.
	/// </summary>
	/// <param name="sender"> The sender. </param>
	/// <param name="args"> The event args. </param>
	[DebuggerNonUserCode]
	public void Handler(object sender, T args)
	{
		var target = _targetReference.Target;
		if (target == null)
		{
			return;
		}

		var callback = (Action<object, T>) Delegate.CreateDelegate(typeof(Action<object, T>), target, _method, true);
		callback?.Invoke(sender, args);
	}

	#endregion
}