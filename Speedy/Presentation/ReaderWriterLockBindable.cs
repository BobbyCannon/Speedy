#region References

using System;
using Speedy.Threading;

#endregion

namespace Speedy.Presentation;

/// <summary>
/// Represents a bindable that is also lockable (thread-safe).
/// </summary>
public class ReaderWriterLockBindable : Bindable, IReaderWriterLock
{
	#region Fields

	private IReaderWriterLock _readerWriterLock;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialize the bindable object.
	/// </summary>
	/// <param name="readerWriterLock"> An optional lock. Defaults to <see cref="ReaderWriterLockTiny" /> if not provided. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public ReaderWriterLockBindable(IReaderWriterLock readerWriterLock = null, IDispatcher dispatcher = null) : base(dispatcher)
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

	/// <summary>
	/// Update the lock provider.
	/// </summary>
	/// <param name="readerWriterLock"> The new lock.</param>
	public void UpdateLock(IReaderWriterLock readerWriterLock)
	{
		var old = _readerWriterLock;
		_readerWriterLock = readerWriterLock;

		if (old is IDisposable d)
		{
			d.Dispose();
		}
	}

	#endregion
}