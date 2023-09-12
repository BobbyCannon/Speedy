#region References

using System;
using System.Threading;

#endregion

namespace Speedy;

/// <summary>
/// Represents a bindable object that is lockable (thread-safe).
/// </summary>
public class LockableBindable : Bindable, IDisposable
{
	#region Fields

	private readonly ReaderWriterLockSlim _cacheLock;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a lockable bindable object.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public LockableBindable(IDispatcher dispatcher = null) : base(dispatcher)
	{
		_cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
	}

	#endregion

	#region Properties

	/// <summary>
	/// True if the object has been disposed otherwise false.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	/// Gets a value that indicates whether the current thread has entered the lock in read mode.
	/// </summary>
	public bool IsReadLocked => _cacheLock.IsReadLockHeld;

	/// <summary>
	/// Gets a value that indicates whether the current thread has entered the lock in upgradeable mode.
	/// </summary>
	public bool IsUpgradeableReadLockHeld => _cacheLock.IsUpgradeableReadLockHeld;

	/// <summary>
	/// Gets a value that indicates whether the current thread has entered the lock in write mode.
	/// </summary>
	public bool IsWriteLocked => _cacheLock.IsWriteLockHeld;

	#endregion

	#region Methods

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Run an action with a read lock.
	/// </summary>
	/// <param name="action"> The action to be executed. </param>
	public void ReadLock(Action action)
	{
		try
		{
			_cacheLock.EnterReadLock();

			action();
		}
		finally
		{
			_cacheLock.ExitReadLock();
		}
	}

	/// <summary>
	/// Run an action with a read lock.
	/// </summary>
	/// <param name="action"> The action to be executed. </param>
	public T ReadLock<T>(Func<T> action)
	{
		try
		{
			_cacheLock.EnterReadLock();
			return action();
		}
		finally
		{
			_cacheLock.ExitReadLock();
		}
	}

	/// <summary>
	/// Process on action in a read lock that can be upgraded to a write lock.
	/// Note: be sure to upgrade on the same thread!
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	public void UpgradeableReadLock(Action action)
	{
		var gotLock = false;

		try
		{
			gotLock = _cacheLock.TryEnterUpgradeableReadLock(2000);

			if (!gotLock)
			{
				throw new TimeoutException("Failed to enter upgradeable read lock");
			}

			action();
		}
		finally
		{
			if (gotLock)
			{
				_cacheLock.ExitUpgradeableReadLock();
			}
		}
	}

	/// <summary>
	/// Process on action in a read lock that can be upgraded to a write lock.
	/// Note: be sure to upgrade on the same thread!
	/// </summary>
	/// <typeparam name="T"> The type the action returns. </typeparam>
	/// <param name="action"> The action to execute. </param>
	/// <returns> The value returned from the action. </returns>
	public T UpgradeableReadLock<T>(Func<T> action)
	{
		try
		{
			_cacheLock.EnterUpgradeableReadLock();
			return action();
		}
		finally
		{
			_cacheLock.ExitUpgradeableReadLock();
		}
	}

	/// <summary>
	/// Run an action with a write lock.
	/// </summary>
	/// <param name="action"> The action to be executed. </param>
	public void WriteLock(Action action)
	{
		try
		{
			_cacheLock.EnterWriteLock();

			action();
		}
		finally
		{
			_cacheLock.ExitWriteLock();
		}
	}

	/// <summary>
	/// Run an action with a write lock.
	/// </summary>
	/// <param name="action"> The action to be executed. </param>
	public T WriteLock<T>(Func<T> action)
	{
		try
		{
			_cacheLock.EnterWriteLock();

			return action();
		}
		finally
		{
			_cacheLock.ExitWriteLock();
		}
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

		IsDisposed = true;

		_cacheLock.Dispose();
	}

	#endregion
}