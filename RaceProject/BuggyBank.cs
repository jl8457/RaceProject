// YOUR NAME HERE
// CSCI 251 - Project 1: Race Condition Detective
// Bug 1: BuggyBank - Fix the race condition in this file

namespace RaceConditionDetective;

/// <summary>
/// A simple bank account that supports deposits and withdrawals.
/// BUG: There is a race condition that causes incorrect balances
/// when multiple threads transfer money simultaneously.
/// </summary>
public class BuggyBank
{
    private decimal _balance;
    private object _depositLock = new object();

    public BuggyBank(decimal initialBalance)
    {
        _balance = initialBalance;
    }

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        // Simulate some processig time
        //Thread.SpinWait(10); //reduces wait from 100 to 10
        lock (_depositLock)
        {
            _balance += amount;
        }
    }
    
    public bool Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        lock (_depositLock)
        {
            if (_balance >= amount)
            {
                decimal current = _balance;
                // Simulate some processing time
                //Thread.SpinWait(10);
                _balance = current - amount;
                return true;
            }
            return false;
        }

    }

    public void Transfer(BuggyBank destination, decimal amount)
    {
        if (Withdraw(amount))
        {
            destination.Deposit(amount);
        }
    }
}
