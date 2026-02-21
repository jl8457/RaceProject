Author: Jinhan Lin

Bug 1: BuggyBank
Race Condition Location:
Lines 28-31 in method Deposit() and lines 42-47 in method Withdraw().

Shared State Involved:
_balance field accessed by multiple threads.

Why It's a Bug:
The deposit operation performs a non-atomic read-modify-write on
_balance, allowing threads to read stale values, compute, and overwrite
each other's updates, leading to lost deposits. The withdraw has a
check-then-act race where after checking sufficient balance, another thread
can withdraw, but the first still subtracts based on stale value, potentially
causing negative balances or lost withdrawals.

Your Fix:
private lock object and enclosed the critical sections in
Deposit and Withdraw with lock statements to ensure atomicity.

Why Your Fix Works:
The per-account lock serializes access to _balance, preventing interleaving
during updates and ensuring each operation reads and writes consistently
without losses.


Bug 2: BuggyCounter
Race Condition Location:
Line 20 in method Increment() and lines 29-31 in method IncrementBy().

Shared State Involved:
_count field accessed by multiple threads.

Why It's a Bug:
The _count++ is a non-atomic read-modify-write operation, so concurrent
threads may read the same value, increment separately, and write back the
same result, losing increments. In IncrementBy, the loop repeats this,
amplifying lost updates under concurrency. This causes the final count to be
lower than the expected total increments.

Your Fix:
Interlocked.Increment for single increments and Interlocked.Add
for adding amounts to ensure atomic updates.

Why Your Fix Works:
Interlocked operations perform the read-modify-write atomically at the
hardware level, preventing races and guaranteeing all increments are
accounted for.


Bug 3: BuggyCache
Race Condition Location:
Lines 30-35 in method GetOrCompute().

Shared State Involved:
_cache dictionary accessed by multiple threads.

Why It's a Bug:
The check-then-act (if (!ContainsKey) then compute and add) allows multiple
threads to detect a missing key simultaneously, all compute the value, and try
to insert, leading to redundant expensive computations. The dictionary is not
thread-safe, risking corruption during concurrent writes. This violates the
cache's intent of computing each key once.

Your Fix:
I added a lock and wrapped the check, compute, insert, and return in a lock
block using TryGetValue.

Why Your Fix Works:
The lock ensures exclusive access, so only one thread computes per key while
others wait, avoiding redundancy and protecting the dictionary.


Bug 4: BuggyLogger
Race Condition Location:
Line 34 in method Log() and lines 47-50 in method Flush().

Shared State Involved:
_buffer StringBuilder accessed by multiple threads.

Why It's a Bug:
Concurrent appends to the shared StringBuilder interleave characters from
different messages, producing garbled output. In flush, between ToString()
and Clear(), new appends can insert content that's partially captured or
lost, leading to incomplete flushes. This causes missing or corrupted log
entries in multi-threaded scenarios.

Your Fix:
I added a lock, synchronized appends in Log, and made the check-ToString-Clear
in Flush atomic under the lock.

Why Your Fix Works:
Locking ensures mutual exclusion for buffer modifications and flushes,
preventing interleaving and guaranteeing complete, atomic captures.


Bug 5: BuggyQueue
Race Condition Location:
Lines 38-39 and 44 in method Enqueue(); lines 60-62 and 68-71 in method
Dequeue().

Shared State Involved:
_queue, _count, _isCompleted accessed by multiple threads.

Why It's a Bug:
Enqueue updates _queue and _count without protection, leading to
inconsistent views; Pulse can occur before waiters enter Wait, causing lost
wakeups and hangs. Dequeue waits then dequeues outside the lock, allowing item
theft by other threads, resulting in null returns despite items or negative
counts. This causes consumers to miss items or deadlock.

Your Fix:
I enclosed all updates, checks, and signals in locks, moving dequeue inside
the lock post-wait.

Why Your Fix Works:
Unified locking makes checks and actions atomic, ensures signals reach
waiters, and prevents theft, eliminating lost wakeups and inconsistencies.