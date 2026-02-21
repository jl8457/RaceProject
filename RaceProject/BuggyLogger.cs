// Jinhan Lin - jl8457
// CSCI 251 - Project 1: Race Condition Detective
// Bug 4: BuggyLogger - Fix the race condition(s) in this file

using System.Text;

namespace RaceConditionDetective;

/// <summary>
/// A logger that buffers messages and periodically flushes to console.
/// BUG: Output is sometimes garbled, incomplete, or causes exceptions
/// due to unsynchronized access to the shared buffer.
/// </summary>
public class BuggyLogger
{
    private readonly StringBuilder _buffer = new();
    private readonly List<string> _flushedMessages = new();
    private bool _isRunning = true;
    private object _lock = new();

    /// <summary>
    /// Gets all messages that have been flushed (for testing).
    /// </summary>
    public IReadOnlyList<string> FlushedMessages => _flushedMessages;

    /// <summary>
    /// Logs a message to the buffer.
    /// </summary>
    public void Log(string message)
    {
        // BUG: Multiple threads appending to StringBuilder without synchronization
        lock (_lock)
        { 
            _buffer.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
        
    }

    /// <summary>
    /// Flushes the buffer contents and clears it.
    /// </summary>
    public void Flush()
    {
        // BUG: Reading and clearing buffer is not atomic
        // Another thread might append between ToString() and Clear()
        if (_buffer.Length > 0)
        {
            string contents = _buffer.ToString();
            lock (_lock)
            {
                _buffer.Clear();
            }
            // Store flushed content (for testing)
            if (!string.IsNullOrWhiteSpace(contents))
            {
                _flushedMessages.Add(contents);
            }
        }
    }

    /// <summary>
    /// Starts a background thread that periodically flushes the buffer.
    /// </summary>
    public Thread StartAutoFlush(int intervalMs = 100)
    {
        var thread = new Thread(() =>
        {
            while (_isRunning)
            {
                Thread.Sleep(intervalMs);
                Flush();
            }
            // Final flush
            Flush();
        });
        thread.Start();
        return thread;
    }

    /// <summary>
    /// Stops the auto-flush thread.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Clears all state (for testing).
    /// </summary>
    public void Reset()
    {
        _buffer.Clear();
        _flushedMessages.Clear();
        _isRunning = true;
    }
}
