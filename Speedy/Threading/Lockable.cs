#region References

#endregion

namespace Speedy.Threading;

/// <summary>
/// Represents a object that is lockable (thread-safe).
/// </summary>
public class Lockable : Notifiable, IReaderWriterLock
{
	#region Fields

	private readonly IReaderWriterLock _readerWriterLock;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a lockable object. Defaults to use <see cref="ReaderWriterLockTiny" />.
	/// </summary>
	public Lockable() : this(null)
	{
	}

	/// <summary>
	/// Initializes a lockable object.
	/// </summary>
	/// <param name="readerWriterLock"> An optional lock. Defaults to <see cref="ReaderWriterLockTiny" /> if non provided. </param>
	public Lockable(IReaderWriterLock readerWriterLock)
	{
		_readerWriterLock = readerWriterLock ?? new ReaderWriterLockTiny();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool IsAwaitingWriteLock => _readerWriterLock.IsAwaitingWriteLock;

	/// <inheritdoc />
	public bool IsReadLockHeld => _readerWriterLock.IsReadLockHeld;

	/// <inheritdoc />
	public bool IsWriteLockHeld => _readerWriterLock.IsWriteLockHeld;

	#endregion

	#region Methods

	/// <inheritdoc />
	public void EnterReadLock()
	{
		_readerWriterLock.EnterReadLock();
	}

	/// <inheritdoc />
	public void EnterUpgradeableReadLock()
	{
		_readerWriterLock.EnterUpgradeableReadLock();
	}

	/// <inheritdoc />
	public void EnterWriteLock()
	{
		_readerWriterLock.EnterWriteLock();
	}

	/// <inheritdoc />
	public void ExitReadLock()
	{
		_readerWriterLock.ExitReadLock();
	}

	/// <inheritdoc />
	public void ExitUpgradeableReadLock()
	{
		_readerWriterLock.ExitUpgradeableReadLock();
	}

	/// <inheritdoc />
	public void ExitWriteLock()
	{
		_readerWriterLock.ExitWriteLock();
	}

	#endregion
}