using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Console;

namespace SynchronizingResourceAccess
{
    class Program
    {
        static Random r = new Random();
        static string Message; // общий ресурс
        static object conch = new object();
        static int Counter; // інший загальний ресурс

        static void MethodA()
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(r.Next(2000));
                Message += "A";
                Write(".");
            }
        }

        static void MethodB()
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(r.Next(2000));
                Message += "B";
                Write(".");
            }
        }

        static void MethodALock()
        {
            lock (conch)
            {
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(r.Next(2000));
                    Message += "A";
                    Write(".");
                }
            }
        }

        static void MethodBLock()
        {
            lock (conch)
            {
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(r.Next(2000));
                    Message += "B";
                    Write(".");
                }
            }
        }

        static void MethodAMonitor()
        {
            try
            {
                Monitor.TryEnter(conch, TimeSpan.FromSeconds(15));
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(r.Next(2000));
                    Message += "A";
                    Interlocked.Increment(ref Counter);
                    Write(".");
                }
            }
            finally
            {
                Monitor.Exit(conch);
            }
        }

        static void MethodBMonitor()
        {
            try
            {
                Monitor.TryEnter(conch, TimeSpan.FromSeconds(15));
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(r.Next(2000));
                    Message += "B";
                    Interlocked.Increment(ref Counter);
                    Write(".");
                }
            }
            finally
            {
                Monitor.Exit(conch);
            }
        }

        static void Run(Action methodA, Action methodB, bool showCounter)
        {
            Message = "";
            Counter = 0;
            WriteLine("Please wait for the tasks to complete.");
            Stopwatch watch = Stopwatch.StartNew();
            Task a = Task.Factory.StartNew(methodA);
            Task b = Task.Factory.StartNew(methodB);
            Task.WaitAll(new Task[] { a, b });
            WriteLine();
            WriteLine($"Results: {Message}.");
            WriteLine($"{watch.ElapsedMilliseconds:#,##0} elapsed milliseconds.");
            if (showCounter)
                WriteLine($"{Counter} string modifications.");
            WriteLine("Press ENTER to continue.");
            ReadLine();
        }

        static void Main(string[] args)
        {
            WriteLine("=== Step 1: without synchronization ===");
            Run(MethodA, MethodB, false);

            WriteLine("=== Step 2: lock ===");
            Run(MethodALock, MethodBLock, false);

            WriteLine("=== Step 3: Monitor.TryEnter + Interlocked ===");
            Run(MethodAMonitor, MethodBMonitor, true);
        }
    }
}