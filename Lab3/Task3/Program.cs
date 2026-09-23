using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int Iterations = 50;

    static void Main(string[] args)
    {
        // Dummy entry point if launched as a target process
        if (args.Length > 0 && args[0] == "--dummy")
        {
            return;
        }

        string currentExecutable = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName;

        // 1. Measure Process Creation Overhead
        var processStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentExecutable,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("--dummy");

            Process? p = Process.Start(startInfo);
            p?.WaitForExit();
        }
        processStopwatch.Stop();

        // 2. Measure Thread Creation Overhead
        var threadStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Thread t = new Thread(() => { /* trivial work */ });
            t.Start();
            t.Join();
        }
        threadStopwatch.Stop();

        double avgProcessMs = processStopwatch.Elapsed.TotalMilliseconds / Iterations;
        double avgThreadMs = threadStopwatch.Elapsed.TotalMilliseconds / Iterations;

        Console.WriteLine($"Average process creation time: {avgProcessMs:F3} ms");
        Console.WriteLine($"Average thread creation time: {avgThreadMs:F3} ms");
        Console.WriteLine($"Process creation was {(avgProcessMs / avgThreadMs):F1}x more expensive than thread creation.");
    }
}