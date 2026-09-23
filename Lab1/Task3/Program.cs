using System;
using System.Diagnostics;
using System.Threading;

class MatrixWorker
{
    double[,] A, B, C;
    int startRow, endRow;

    public MatrixWorker(double[,] A, double[,] B, double[,] C, int startRow, int endRow)
    {
        this.A = A;
        this.B = B;
        this.C = C;
        this.startRow = startRow;
        this.endRow = endRow;
    }

    public void Run()
    {
        int n = A.GetLength(0);

        for (int i = startRow; i < endRow; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sum = 0;

                for (int k = 0; k < n; k++)
                {
                    sum += A[i, k] * B[k, j];
                }

                C[i, j] = sum;
            }
        }
    }
}

class ParallelMatrixMultiplication
{
    static void Main()
    {
        int[] sizes = { 200, 400, 600 };

        foreach (int n in sizes)
        {
            double[,] A = new double[n, n];
            double[,] B = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    A[i, j] = 1;
                    B[i, j] = 1;
                }
            }

            double total = 0;

            Console.WriteLine("\n" + n + " x " + n);

            for (int run = 1; run <= 3; run++)
            {
                double[,] C = new double[n, n];

                int middle = n / 2;

                MatrixWorker w1 = new MatrixWorker(A, B, C, 0, middle);
                MatrixWorker w2 = new MatrixWorker(A, B, C, middle, n);

                Thread t1 = new Thread(w1.Run);
                Thread t2 = new Thread(w2.Run);

                Stopwatch sw = Stopwatch.StartNew();

                t1.Start();
                t2.Start();

                t1.Join();
                t2.Join();

                sw.Stop();

                double time = sw.Elapsed.TotalMilliseconds;
                total += time;

                Console.WriteLine("Run " + run + ": " + time + " ms");
            }

            Console.WriteLine("Average: " + total / 3 + " ms");
        }
    }
}