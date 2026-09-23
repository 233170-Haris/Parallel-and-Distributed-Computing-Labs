using System;
using System.Threading;

class Program
{
    static void Worker()
    {
        Thread.Sleep(200);
    }

    static void Main()
    {
        Thread t = new Thread(Worker);
        Console.WriteLine($"After creation: Thread state = {t.ThreadState}"); // New

        t.Start();
        Console.WriteLine($"After start: Thread state = {t.ThreadState}"); // Runnable/Ready or Running

        Thread.Sleep(50);
        Console.WriteLine($"After 50ms sleep: Thread state = {t.ThreadState}"); // Blocked/Waiting

        t.Join();
        Console.WriteLine($"After join: Thread state = {t.ThreadState}"); // Terminated
    }
}