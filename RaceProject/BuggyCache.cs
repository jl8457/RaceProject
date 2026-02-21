// Jinhan Lin
// CSCI 251 - Project 1: Race Condition Detective
// Bug 3: BuggyCache - Fix the race condition in this file

namespace RaceConditionDetective;

/// <summary>
/// A cache that computes and stores expensive values on first access.
/// BUG: The expensive computation sometimes runs multiple times
/// for the same key due to a check-then-act race condition.
/// </summary>
public class BuggyCache
{
    private readonly Dictionary<string, int> _cache = new();
    private int _computeCount = 0;
    private object _lock = new object();

    /// <summary>
    /// Gets the number of times the expensive computation was performed.
    /// Used for testing - ideally each key should only compute once.
    /// </summary>
    public int ComputeCount => _computeCount;

    /// <summary>
    /// Gets a value from the cache, computing it if not present.
    /// </summary>
    public int GetOrCompute(string key)
    {
        // BUG: Check-then-act race condition!
        // Multiple threads can see the key as missing and all compute it.
        lock (_lock)
        {
            if (!_cache.ContainsKey(key))
            {
                int value = ExpensiveComputation(key);
                _cache[key] = value;
            }
            return _cache[key];
        }
    }

    /// <summary>
    /// Simulates an expensive computation that we want to cache.
    /// </summary>
    private int ExpensiveComputation(string key)
    {
        // Track how many times we compute (for testing)
        Interlocked.Increment(ref _computeCount);

        // Simulate expensive work
        Thread.Sleep(50);

        // Return a deterministic value based on key
        return key.GetHashCode();
    }

    public void Clear()
    {
        _cache.Clear();
        _computeCount = 0;
    }
}
