// YOUR NAME HERE
// CSCI 251 - Project 1: Race Condition Detective
// Bug 2: BuggyCounter - Fix the race condition in this file

namespace RaceConditionDetective;

/// <summary>
/// A counter that can be incremented by multiple threads.
/// BUG: The final count is often less than expected because
/// increment is not atomic.
/// </summary>
public class BuggyCounter
{
    private int _count = 0;

    public int Count => _count;

    public void Increment()
    {
        // BUG: This looks atomic but it's actually read-modify-write!
        // _count++ is equivalent to: temp = _count; temp = temp + 1; _count = temp;
        Interlocked.Increment(ref _count);
    }

    public void IncrementBy(int amount)
    {
        // BUG: Same problem, just more obvious
        //Interlocked.Add(ref _count, amount);
        _count += amount;
    }

    public void Reset()
    {
        _count = 0;
    }
}
