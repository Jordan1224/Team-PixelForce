using System;
using System.Numerics;
using System.Collections.Generic;

/// <summary>
/// Static utility to avoid Vector2 allocations.
/// Reuses temporary vectors for calculations.
/// </summary>
public static class VectorPool
{
    [ThreadStatic]
    private static Stack<Vector2> _pool = new Stack<Vector2>(16);

    /// <summary>
    /// Get a temporary vector. Must call Return() after use.
    /// </summary>
    public static Vector2 Get()
    {
        return _pool.Count > 0 ? _pool.Pop() : Vector2.Zero;
    }

    /// <summary>
    /// Get a temporary vector with initial value.
    /// </summary>
    public static Vector2 Get(float x, float y)
    {
        var v = Get();
        return new Vector2(x, y);
    }

    /// <summary>
    /// Return a temporary vector to the pool.
    /// </summary>
    public static void Return(Vector2 vector)
    {
        if (_pool.Count < 32)
            _pool.Push(vector);
    }

    /// <summary>
    /// Clear the pool.
    /// </summary>
    public static void Clear()
    {
        _pool.Clear();
    }
}