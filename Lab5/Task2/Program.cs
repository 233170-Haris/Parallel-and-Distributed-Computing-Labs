using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static int[] buffer;
    static int inPos = 0;
    static int outPos = 0;

    static readonly object gate = new object();

    static SemaphoreSlim emptySlots;
    static SemaphoreSlim fullSlots;

    static int nextItem = 1;
    static int totalItems = 1000;

    static int[] consumedItems;

    static void Producer()
    {
        while (true)
        {
            int item;

            // Get a unique item number
            lock (gate)
            {
                if (nextItem > totalItems)
                    return;

                item = nextItem;
                nextItem++;
            }

            // Wait for an empty slot
            emptySlots.Wait();

            // Insert item
            lock (gate)
            {
                buffer[inPos] = item;
                inPos = (inPos + 1) % buffer.Length;
            }

            // Signal that an item is available
            fullSlots.Release();
        }
    }

    static void Consumer()
    {
        while (true)
        {
            // Wait for an available item
            fullSlots.Wait();

            int item;

            // Remove item
            lock (gate)
            {
                item = buffer[outPos];
                outPos = (outPos + 1) % buffer.Length;
            }

            // Signal that a buffer slot is free
            emptySlots.Release();

            // -1 means stop
            if (item == -1)
                return;

            lock (gate)
            {
                consumedItems[item]++;
            }
        }
    }

    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine(
                "Usage: dotnet run --no-build -c Release -- <producers> <consumers> <capacity>"
            );
            return;
        }

        int producers = int.Parse(args[0]);
        int consumers = int.Parse(args[1]);
        int capacity = int.Parse(args[2]);

        buffer = new int[capacity];
        consumedItems = new int[totalItems + 1];

        emptySlots = new SemaphoreSlim(capacity, capacity);
        fullSlots = new SemaphoreSlim(0, capacity);

        Thread[] producerThreads = new Thread[producers];
        Thread[] consumerThreads = new Thread[consumers];

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Start producers
        for (int i = 0; i < producers; i++)
        {
            producerThreads[i] = new Thread(Producer);
            producerThreads[i].Start();
        }

        // Start consumers
        for (int i = 0; i < consumers; i++)
        {
            consumerThreads[i] = new Thread(Consumer);
            consumerThreads[i].Start();
        }

        // Wait for all producers
        foreach (Thread thread in producerThreads)
        {
            thread.Join();
        }

        // Send one stop signal to each consumer
        for (int i = 0; i < consumers; i++)
        {
            emptySlots.Wait();

            lock (gate)
            {
                buffer[inPos] = -1;
                inPos = (inPos + 1) % buffer.Length;
            }

            fullSlots.Release();
        }

        // Wait for all consumers
        foreach (Thread thread in consumerThreads)
        {
            thread.Join();
        }

        stopwatch.Stop();

        // Calculate lost and duplicated items
        int lost = 0;
        int duplicated = 0;

        for (int i = 1; i <= totalItems; i++)
        {
            if (consumedItems[i] == 0)
            {
                lost++;
            }
            else if (consumedItems[i] > 1)
            {
                duplicated += consumedItems[i] - 1;
            }
        }

        Console.WriteLine($"Lost: {lost}");
        Console.WriteLine($"Duplicated: {duplicated}");
        Console.WriteLine($"Time (ms): {stopwatch.ElapsedMilliseconds}");
    }
}