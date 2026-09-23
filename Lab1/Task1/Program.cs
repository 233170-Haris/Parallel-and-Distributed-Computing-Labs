using System.Diagnostics;

int size = 12_000_000;
int[] data = new int[size];

for (int i = 0; i < size; i++)
{
    data[i] = 1;
}

Stopwatch stopwatch = Stopwatch.StartNew();

long sum = 0;

for (int i = 0; i < size; i++)
{
    sum += data[i];
}

stopwatch.Stop();

double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

Console.WriteLine($"Array size: {size}");
Console.WriteLine($"Sum: {sum}");
Console.WriteLine($"Execution time: {elapsedMs} ms");