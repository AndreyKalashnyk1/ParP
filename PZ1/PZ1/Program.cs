using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Console;

namespace PZ1_Tasks
{
    class Program
    {
        static void MethodA()
        {
            WriteLine("Starting Method A...");
            Thread.Sleep(3000); // імітація роботи протягом трьох секунд
            WriteLine("Finished Method A.");
        }

        static void MethodB()
        {
            WriteLine("Starting Method B...");
            Thread.Sleep(2000); // імітація роботи протягом двох секунд
            WriteLine("Finished Method B.");
        }

        static void MethodC()
        {
            WriteLine("Starting Method C...");
            Thread.Sleep(1000); // імітація роботи протягом однієї секунди
            WriteLine("Finished Method C.");
        }

        static void Step1()
        {
            var timer = Stopwatch.StartNew();
            WriteLine("Running methods synchronously on one thread.");
            MethodA();
            MethodB();
            MethodC();
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            WriteLine("Press ENTER to end.");
            ReadLine();
        }

        static void Step2()
        {
            var timer = Stopwatch.StartNew();
            //WriteLine("Running methods synchronously on one thread.");
            //MethodA();
            //MethodB();
            //MethodC();
            WriteLine("Running methods asynchronously on multiple threads.");
            Task taskA = new Task(MethodA);
            taskA.Start();
            Task taskB = Task.Factory.StartNew(MethodB);
            Task taskC = Task.Run(new Action(MethodC));
            WriteLine($"{timer.Elapsed.TotalMilliseconds:0.000}ms elapsed.");
            WriteLine("Press ENTER to end.");
            ReadLine();
        }

        static void Step3()
        {
            var timer = Stopwatch.StartNew();
            //WriteLine("Running methods synchronously on one thread.");
            //MethodA();
            //MethodB();
            //MethodC();
            WriteLine("Running methods asynchronously on multiple threads.");
            Task taskA = new Task(MethodA);
            taskA.Start();
            Task taskB = Task.Factory.StartNew(MethodB);
            Task taskC = Task.Run(new Action(MethodC));
            Task[] tasks = { taskA, taskB, taskC };
            Task.WaitAll(tasks);
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            WriteLine("Press ENTER to end.");
            ReadLine();
        }

        static void Main(string[] args)
        {
            WriteLine("=== Step 1: synchronous ===");
            Step1();

            WriteLine();
            WriteLine("=== Step 2: Task, no wait ===");
            Step2();

            WriteLine();
            WriteLine("=== Step 3: Task + WaitAll ===");
            Step3();
        }
    }
}