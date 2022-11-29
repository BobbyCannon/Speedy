#region References

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

#endregion

namespace Speedy.Commands;

/// <summary>
/// A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default return value for the CanExecute method is 'true'.
/// </summary>
public class RelayCommand : ICommand
{
	#region Fields

	private readonly MethodInfo _canExecuteCallback;
	private readonly WeakReference<object> _canExecuteReference;
	private readonly MethodInfo _executeCallback;
	private readonly WeakReference<object> _executeReference;

	#endregion

	#region Constructors

	/// <summary>
	/// Creates a new command.
	/// </summary>
	/// <param name="execute"> The execution logic. </param>
	/// <param name="canExecute"> The execution status logic. </param>
	public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
	{
		_executeReference = new WeakReference<object>(execute.Target);
		_executeCallback = execute.GetMethodInfo();

		if (canExecute != null)
		{
			_canExecuteReference = new WeakReference<object>(canExecute.Target);
			_canExecuteCallback = canExecute.GetMethodInfo();
		}
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	[DebuggerStepThrough]
	public bool CanExecute(object parameter)
	{
		if (_canExecuteReference == null)
		{
			return true;
		}

		if (_canExecuteReference.TryGetTarget(out var action))
		{
			return (bool) _canExecuteCallback.Invoke(action, new[] { parameter });
		}

		return false;
	}

	/// <inheritdoc />
	public void Execute(object parameter)
	{
		if (_executeReference.TryGetTarget(out var action))
		{
			_executeCallback.Invoke(action, new[] { parameter });
		}
	}

	/// <summary>
	/// Refresh the command state.
	/// </summary>
	public void Refresh()
	{
		OnCanExecuteChanged();
	}

	/// <summary>
	/// Overridable can execute event.
	/// </summary>
	protected virtual void OnCanExecuteChanged()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler CanExecuteChanged;

	#endregion
}