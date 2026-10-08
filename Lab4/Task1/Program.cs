using System;
using System.Threading;

class Program
{
    const int Numthreads = 4;
    const int IncrementsPerThread = 1_000_000;
    static long counter = 0;

    static void Worker()
    {
        for (int i = 0; i < IncrementsPerThread; i++)
        {
            counter++;
        }

    }

    static void Main()
    {
        Thread[] threads = new Thread[Numthreads];
        for(int i = 0 ; i < Numthreads; i++)
        {
            threads[i] = new Thread(Worker);
            threads[i].Start();
        }

        for (int i = 0; i < Numthreads; i++)
        {
            threads[i].Join();
        }

        long expected = Numthreads * IncrementsPerThread;
        Console.WriteLine($"Expected counter value: {expected}");
        Console.WriteLine($"Actual counter value: {counter}");
        Console.WriteLine($"Lost Updates: {expected - counter}");

    }

}