using System.Collections;

namespace Muninn.Kernel.Shared;

public class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly HashSet<T> _hashSet;

    public ConcurrentHashSet()
    {
        _hashSet = [];
    }

    public ConcurrentHashSet(IEnumerable<T> enumerable)
    {
        _hashSet = [..enumerable];
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();

            try
            {
                return _hashSet.Count;
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                {
                    _lock.ExitReadLock();
                }
            }
        }
    }

    public bool Add(T item)
    {
        _lock.EnterWriteLock();

        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();

        try
        {
            _hashSet.Clear();
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();

        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            if (_lock.IsReadLockHeld)
            {
                _lock.ExitReadLock();
            }
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();

        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock.Dispose();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _hashSet.GetEnumerator();
    }

    ~ConcurrentHashSet()
    {
        Dispose(false);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
