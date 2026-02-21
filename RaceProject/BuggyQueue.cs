// Jinhan Lin

namespace RaceConditionDetective;

/// <summary>
/// A producer-consumer queue where producers add items and consumers take them.
/// BUG: Consumers sometimes receive null when items are available,
/// or the program hangs due to lost wakeups.
/// </summary>
public class BuggyQueue<T> where T : class
{
    private readonly Queue<T> _queue = new();
    private readonly object _lock = new();
    private int _count = 0;
    private bool _isCompleted = false;

    /// <summary>
    /// Gets the current number of items in the queue.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Indicates whether the queue has been marked as complete.
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// Adds an item to the queue.
    /// </summary>
    public void Enqueue(T item)
    {
        // BUG: Pulse might be called when no one is waiting
        // and the wakeup is lost
        lock (_lock)
        {
            if (_isCompleted)
                throw new InvalidOperationException("Queue is completed");

            // BUG: _count update is not synchronized with _queue update
            _queue.Enqueue(item);
            _count++;
            Monitor.Pulse(_lock);
        }
    }

    /// <summary>
    /// Removes and returns an item from the queue.
    /// Blocks if the queue is empty until an item is available or queue is completed.
    /// Returns null if queue is completed and empty.
    /// </summary>
    public T? Dequeue()
    {
        lock (_lock)
        {
            // BUG: Checking _count outside the same lock that protects _queue
            while (_count == 0 && !_isCompleted)
            {
                Monitor.Wait(_lock);
            }
            // BUG: By the time we get here, another thread might have taken the item
            if (_count == 0)
                return null;

            // BUG: This is not protected by the lock!
            var item = _queue.Dequeue();
            _count--;
            return item;
        }
    }

    /// <summary>
    /// Marks the queue as complete. No more items can be added.
    /// Consumers waiting on an empty queue will return null.
    /// </summary>
    public void Complete()
    {
        _isCompleted = true;
        lock (_lock)
        {
            // Wake up all waiting consumers
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    /// Resets the queue (for testing).
    /// </summary>
    public void Reset()
    {
        _queue.Clear();
        _count = 0;
        _isCompleted = false;
    }
}
