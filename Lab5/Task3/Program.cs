using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static readonly object gate = new object();

    static bool dataReady = false;
    static int data = 0;

    static int blockedCount = 0;

    static void Worker()
    {
        lock (gate)
        {
            while (!dataReady)
            {
                blockedCount++;
                Monitor.Wait(gate);
            }

            Console.WriteLine($"Worker received: {data}");
        }
    }

    static void Publish(int value)
    {
        lock (gate)
        {
            data = value;
            dataReady = true;

            Monitor.PulseAll(gate);
        }
    }

    static void RunCondition(int workerCount, bool signalFirst)
    {
        Thread[] workers = new Thread[workerCount];

        for (int i = 0; i < workerCount; i++)
        {
            workers[i] = new Thread(Worker);
            workers[i].Start();
        }

        if (signalFirst)
        {
            Thread.Sleep(50);
        }

        Thread.Sleep(100);

        Publish(42);

        foreach (Thread worker in workers)
        {
            worker.Join();
        }
    }

    static void RunSpin()
    {
        Thread worker = new Thread(() =>
        {
            while (!dataReady)
            {
                Thread.Yield();
            }

            Console.WriteLine($"Worker received: {data}");
        });

        worker.Start();

        Thread.Sleep(100);

        data = 42;
        dataReady = true;

        worker.Join();
    }

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("A: cond");
            Console.WriteLine("B: cond 1 signal-first");
            Console.WriteLine("C: cond 3");
            Console.WriteLine("D: spin");
            return;
        }

        Stopwatch wall = Stopwatch.StartNew();
        TimeSpan cpuStart = Process.GetCurrentProcess().TotalProcessorTime;

        switch (args[0].ToLower())
        {
            case "cond":
                int workers = 1;

                if (args.Length >= 2 && args[1] == "3")
                    workers = 3;

                bool signalFirst =
                    args.Length >= 3 && args[2] == "signal-first";

                RunCondition(workers, signalFirst);
                break;

            case "spin":
                RunSpin();
                break;

            default:
                Console.WriteLine("Unknown scenario.");
                return;
        }

        wall.Stop();

        TimeSpan cpuEnd = Process.GetCurrentProcess().TotalProcessorTime;
        double cpuMs = (cpuEnd - cpuStart).TotalMilliseconds;

        Console.WriteLine($"Times blocked in wait: {blockedCount}");
        Console.WriteLine($"Value received: {data}");
        Console.WriteLine($"Wall time (ms): {wall.ElapsedMilliseconds}");
        Console.WriteLine($"CPU time (ms): {cpuMs:F2}");
    }
}