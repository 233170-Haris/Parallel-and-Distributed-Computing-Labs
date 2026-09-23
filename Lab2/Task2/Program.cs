using System;
using System.Threading;

class Program
{
    static void Worker(object? arg)
    {
        long id = Convert.ToInt64(arg);
        Console.WriteLine($"Thread {id} running on CPU {Thread.GetCurrentProcessorId()}");

    }

static void Main()
    {
        int numCores = Environment.ProcessorCount;
        Console.WriteLine($"Number of CPU cores Detected: {numCores}");
        Thread[] threads = new Thread[numCores];

        for (int i = 0; i < numCores; i++)
        {
            long id = i + 1;
            threads[i] = new Thread(Worker);
            threads[i].Start(i+1);
        }

        for (int i = 0; i < numCores; i++)
        {
            threads[i].Join();
        }
        Console.WriteLine($"Total number of threads created: {numCores}");
    }
}
