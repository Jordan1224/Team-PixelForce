using System;
using System.Collections.Generic;

/// <summary>
/// Generic object pool to reduce allocations.
/// Reuse objects instead of creating/destroying them.
/// </summary>
public class ObjectPool<T> where T : new()
{
    private Stack<T> _available;
    private HashSet<T> _inUse;
    private Func<T> _factory;
    private Action<T> _onGet;
    private Action<T> _onReturn;

    public int TotalCount { get; private set; }
    public int AvailableCount => _available.Count;

    public ObjectPool(int initialCapacity = 10, Func<T> factory = null, 
        Action<T> onGet = null, Action<T> onReturn = null)
    {
        _factory = factory ?? (() => new T());
        _onGet = onGet;
        _onReturn = onReturn;
        _available = new Stack<T>(initialCapacity);
        _inUse = new HashSet<T>();

        // Pre-allocate
        for (int i = 0; i < initialCapacity; i++)
        {
            _available.Push(_factory());
        }
        TotalCount = initialCapacity;
    }

    public T Get()
    {
        T item;
        if (_available.Count > 0)
        {
            item = _available.Pop();
        }
        else
        {
            item = _factory();
            TotalCount++;
        }

        _inUse.Add(item);
        _onGet?.Invoke(item);
        return item;
    }

    public void Return(T item)
    {
        if (!_inUse.Contains(item)) return;

        _inUse.Remove(item);
        _onReturn?.Invoke(item);
        _available.Push(item);
    }

    public void Clear()
    {
        _available.Clear();
        _inUse.Clear();
        TotalCount = 0;
    }
}