using System;
using System.Diagnostics;
using System.Threading;

class Node
{
    public int Value;
    public Node Next;

    public Node(int value)
    {
        Value = value;
        Next = null;
    }
}

class LockFreeStack
{
    private Node head;

    // Push using CAS
    public void Push(int value)
    {
        Node newNode = new Node(value);

        while (true)
        {
            Node oldHead = Volatile.Read(ref head);

            newNode.Next = oldHead;

            Node result = Interlocked.CompareExchange(
                ref head,
                newNode,
                oldHead
            );

            if (result == oldHead)
            {
                return;
            }
        }
    }

    // Pop using CAS
    public bool Pop(out int value)
    {
        while (true)
        {
            Node oldHead = Volatile.Read(ref head);

            if (oldHead == null)
            {
                value = 0;
                return false;
            }

            Node newHead = oldHead.Next;

            Node result = Interlocked.CompareExchange(
                ref head,
                newHead,
                oldHead
            );

            if (result == oldHead)
            {
                value = oldHead.Value;
                return true;
            }
        }
    }

    public bool IsEmpty()
    {
        return Volatile.Read(ref head) == null;
    }
}

class Program
{
    const int NumThreads = 4;
    const int ItemsPerThread = 1000;

    static LockFreeStack stack = new LockFreeStack();

    static int[] popped = new int[NumThreads * ItemsPerThread];

    static int poppedCount = 0;

    static void Worker(int threadId)
    {
        int start = threadId * ItemsPerThread + 1;

        // Push items
        for (int i = 0; i < ItemsPerThread; i++)
        {
            stack.Push(start + i);
        }

        // Pop items
        for (int i = 0; i < ItemsPerThread; i++)
        {
            int value;

            while (!stack.Pop(out value))
            {
                Thread.Yield();
            }

            int index = Interlocked.Increment(ref poppedCount) - 1;

            Interlocked.Exchange(ref popped[index], value);
        }
    }

    static void Main()
    {
        Thread[] threads = new Thread[NumThreads];

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Create and start threads
        for (int i = 0; i < NumThreads; i++)
        {
            int id = i;

            threads[i] = new Thread(() =>
            {
                Worker(id);
            });

            threads[i].Start();
        }

        // Wait for all threads
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();

        int totalItems = NumThreads * ItemsPerThread;

        bool[] seen = new bool[totalItems + 1];

        int lost = 0;
        int duplicated = 0;

        // Check popped items
        for (int i = 0; i < poppedCount; i++)
        {
            int value = popped[i];

            if (value >= 1 && value <= totalItems)
            {
                if (seen[value])
                {
                    duplicated++;
                }
                else
                {
                    seen[value] = true;
                }
            }
        }

        // Check for lost items
        for (int value = 1; value <= totalItems; value++)
        {
            if (!seen[value])
            {
                lost++;
            }
        }

        Console.WriteLine($"Nodes (total): {totalItems}");
        Console.WriteLine($"Lost: {lost}");
        Console.WriteLine($"Duplicated: {duplicated}");
        Console.WriteLine($"Stack empty: {(stack.IsEmpty() ? "Y" : "N")}");
        Console.WriteLine($"Time (ms): {stopwatch.ElapsedMilliseconds}");
    }
}