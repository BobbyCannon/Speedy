namespace Speedy.Threading;

/// <summary>
/// Represents a reader writer lock.
/// </summary>
public interface IReaderWriterLock
{
	#region Properties

	/// <summary>
	/// Gets a value that indicates a thread is waiting to get a write lock.
	/// </summary>
	bool IsAwaitingWriteLock { get; }

	/// <summary>
	/// Gets a value that indicates locked in read mode.
	/// </summary>
	bool IsReadLockHeld { get; }

	/// <summary>
	/// Gets a value that indicates locked in write mode.
	/// </summary>
	bool IsWriteLockHeld { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Enter the lock in read mode.
	/// </summary>
	void EnterReadLock();

	/// <summary>
	/// Enter the lock in read mode that is upgradable to write.
	/// </summary>
	void EnterUpgradeableReadLock();

	/// <summary>
	/// Enter the lock in write mode.
	/// </summary>
	void EnterWriteLock();

	/// <summary>
	/// Exit the read lock.
	/// </summary>
	void ExitReadLock();

	/// <summary>
	/// Exit the upgradeable read lock.
	/// </summary>
	void ExitUpgradeableReadLock();

	/// <summary>
	/// Exit the write lock.
	/// </summary>
	void ExitWriteLock();

	#endregion
}