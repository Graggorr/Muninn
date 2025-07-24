namespace Muninn.Kernel;

internal class Locker
{
    private readonly ReaderWriterLock _locker = new();
    private readonly TimeSpan _lockLifetime = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _longLockLifetime = TimeSpan.FromMinutes(1);

    public bool IsWriteLocked => _locker.IsWriterLockHeld;

    public bool IsReadLocked => _locker.IsReaderLockHeld;

    public void WriteLock()
    {
        _locker.AcquireWriterLock(_lockLifetime);
    }

    public void WriteLockLong()
    {
        _locker.AcquireWriterLock(_longLockLifetime);
    }

    public void ReadLock()
    {
        _locker.AcquireReaderLock(_lockLifetime);
    }

    public void WriteReleaseLock()
    {
        if (IsWriteLocked)
        {
            _locker.ReleaseWriterLock();
        }
    }

    public void ReadReleaseLock()
    {
        if (IsReadLocked)
        {
            _locker.ReleaseReaderLock();
        }
    }
}
