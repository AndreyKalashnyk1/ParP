using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using static System.Console;

namespace InvokeEx
{
    class Program
    {
        static void Step1()
        {
            // invoke actions described by lambda expressions
            Parallel.Invoke(
               () => Console.WriteLine("Action 1"),
               () => Console.WriteLine("Action 2"),
               () => Console.WriteLine("Action 3"));

            // explicitly create an array of actions
            Action[] actions = new Action[3];
            actions[0] = new Action(() => Console.WriteLine("Action 4"));
            actions[1] = new Action(() => Console.WriteLine("Action 5"));
            actions[2] = new Action(() => Console.WriteLine("Action 6"));

            // invoke the actions array
            Parallel.Invoke(actions);

            // create the same effect using tasks explicitly
            Task parent = Task.Factory.StartNew(() => {
                foreach (Action action in actions) {
                    Task.Factory.StartNew(action,
                        TaskCreationOptions.AttachedToParent);
                }
            });
            // wait for the task to finish
            parent.Wait();
        }

        static void Step2()
        {
            double[] resultData = new double[10000000];
            int parts = 4;
            int size = resultData.Length / parts;
            Action[] actions = new Action[parts];
            for (int p = 0; p < parts; p++)
            {
                int start = p * size;
                int end = (p + 1) * size;
                actions[p] = new Action(() => {
                    for (int i = start; i < end; i++)
                    {
                        resultData[i] = Math.Pow(i, 2);
                    }
                });
            }

            var timer = Stopwatch.StartNew();
            WriteLine("Running Parallel.Invoke with 4 actions.");
            Parallel.Invoke(actions);
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
        }

        static void Main(string[] args)
        {
            WriteLine("=== Step 1: Parallel.Invoke ===");
            Step1();
            WriteLine("Press ENTER to continue.");
            ReadLine();

            WriteLine("=== Step 2: task 4 with Parallel.Invoke ===");
            Step2();

            // wait for input before exiting
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }
    }
}
