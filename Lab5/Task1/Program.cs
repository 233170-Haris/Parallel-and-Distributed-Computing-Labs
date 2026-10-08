using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int BufferSize = 64;
    const int TotalItems = 100;

    static int[] buffer = new int[BufferSize];

    static int inPos = 0;
    static int outPos = 0;
    static int count = 0;

    static readonly object gate = new object();

    static int producerRetries = 0;
    static int consumerRetries = 0;
    static int orderErrors = 0;

    static long producedChecksum = 0;
    static long consumedChecksum = 0;

    static void Producer()
    {
        for (int item = 1; item <= TotalItems; item++)
        {
            while (true)
            {
                lock (gate)
                {
                    if (count < BufferSize)
                    {
                        buffer[inPos] = item;
                        inPos = (inPos + 1) % BufferSize;
                        count++;

                        producedChecksum += item;

                        break;
                    }
                }

                producerRetries++;
                Thread.Yield();
            }
        }
    }

    static void Consumer()
    {
        for (int expected = 1; expected <= TotalItems; expected++)
        {
            int item;

            while (true)
            {
                lock (gate)
                {
                    if (count > 0)
                    {
                        item = buffer[outPos];
                        outPos = (outPos + 1) % BufferSize;
                        count--;

                        consumedChecksum += item;

                        break;
                    }
                }

                consumerRetries++;
                Thread.Yield();
            }

            // Check ordering
            if (item != expected)
            {
                orderErrors++;
            }
        }
    }

    static void Main()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Thread producer = new Thread(Producer);
        Thread consumer = new Thread(Consumer);

        producer.Start();
        consumer.Start();

        producer.Join();
        consumer.Join();

        stopwatch.Stop();

        bool checksumOK = producedChecksum == consumedChecksum;

        Console.WriteLine($"Items Checksum OK: {(checksumOK ? "Y" : "N")}");
        Console.WriteLine($"Order errors: {orderErrors}");
        Console.WriteLine($"Producer retries: {producerRetries}");
        Console.WriteLine($"Consumer retries: {consumerRetries}");
        Console.WriteLine($"Time (ms): {stopwatch.ElapsedMilliseconds}");

        if (checksumOK && orderErrors == 0)
        {
            Console.WriteLine("SUCCESS: No items lost, duplicated, or reordered.");
        }
        else
        {
            Console.WriteLine("FAILURE: Buffer data was incorrect.");
        }
    }
}