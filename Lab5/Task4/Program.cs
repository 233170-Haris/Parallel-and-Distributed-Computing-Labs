using System;
using System.Diagnostics;
using System.Threading;

class PhaseBarrier
{
    private readonly object gate = new object();
    private readonly int parties;

    private int waiting = 0;
    private int generation = 0;

    public PhaseBarrier(int parties)
    {
        this.parties = parties;
    }

    public void SignalAndWait()
    {
        lock (gate)
        {
            int myGeneration = generation;

            waiting++;

            if (waiting == parties)
            {
                waiting = 0;
                generation++;

                Monitor.PulseAll(gate);
            }
            else
            {
                while (myGeneration == generation)
                {
                    Monitor.Wait(gate);
                }
            }
        }
    }
}

class Program
{
    const int DefaultWorkers = 4;
    const int Rounds = 5;
    const int Cells = 1000;

    static int workers;
    static int[][] results;
    static PhaseBarrier barrier;

    static void Worker(int id, bool useBarrier)
    {
        for (int round = 0; round < Rounds; round++)
        {
            // Simulate computation for this round
            for (int cell = 0; cell < Cells; cell++)
            {
                results[id][cell] = round + 1;
            }

            // Synchronize all workers before starting next round
            if (useBarrier)
            {
                barrier.SignalAndWait();
            }
        }
    }

    static void Main(string[] args)
    {
        workers = DefaultWorkers;
        bool useBarrier = true;

        if (args.Length > 0)
        {
            if (args[0] == "--no-barrier")
            {
                useBarrier = false;
            }
            else if (int.TryParse(args[0], out int parsedWorkers))
            {
                workers = parsedWorkers;
            }
        }

        results = new int[workers][];

        for (int i = 0; i < workers; i++)
        {
            results[i] = new int[Cells];
        }

        barrier = new PhaseBarrier(workers);

        Thread[] threads = new Thread[workers];

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < workers; i++)
        {
            int id = i;

            threads[i] = new Thread(() =>
            {
                Worker(id, useBarrier);
            });

            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();

        int mismatches = 0;

        // All workers should have completed the same final round.
        int expected = Rounds;

        for (int worker = 0; worker < workers; worker++)
        {
            for (int cell = 0; cell < Cells; cell++)
            {
                if (results[worker][cell] != expected)
                {
                    mismatches++;
                }
            }
        }

        Console.WriteLine($"Workers: {workers}");
        Console.WriteLine($"With barrier: {(useBarrier ? "Y" : "N")}");
        Console.WriteLine($"Correct: {(mismatches == 0 ? "Y" : "N")}");
        Console.WriteLine($"Time (ms): {stopwatch.ElapsedMilliseconds}");
        Console.WriteLine($"Mismatching cells: {mismatches}");
        Console.WriteLine($"ProcessorCount: {Environment.ProcessorCount}");
    }
}