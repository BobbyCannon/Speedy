#region References

using System.Threading;

#endregion

namespace Speedy.Threading;

/// <inheritdoc cref="IReaderWriterLock" />
public class ReaderWriterLockSlimProxy : ReaderWriterLockSlim, IReaderWriterLock
{
	#region Properties

	/// <inheritdoc />
	public bool IsAwaitingWriteLock => WaitingWriteCount > 0;

	#endregion
}