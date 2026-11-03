using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using static System.Console;

namespace par_for_ex
{
    class Example_01
    {
        static void Step1(int size)
        {
            WriteLine($"Array size: {size:#,##0}");
            // create the arrays to hold the data and the results
            int[] dataItems = new int[size];
            double[] resultItems = new double[size];
            // create the data items
            for (int i = 0; i < dataItems.Length; i++)
            {
                dataItems[i] = i;
            }
            var timer = Stopwatch.StartNew();
            WriteLine("Running methods on one thread.");

            for (int i = 0; i < dataItems.Length; i++)
            {
                resultItems[i] = Math.Pow(dataItems[i], 2);
            }
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            // process the data in a parallel for loop
            timer = Stopwatch.StartNew();
            WriteLine("Running methods on 4 thread.");
            Parallel.For(0, dataItems.Length, (index) => {
                resultItems[index] = Math.Pow(dataItems[index], 2);
            });
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
        }

        static void Step2()
        {
            WriteLine($"Logical processors: {Environment.ProcessorCount}");
            int[] dataItems = new int[1000000];
            double[] resultItems = new double[1000000];
            for (int i = 0; i < dataItems.Length; i++)
            {
                dataItems[i] = i;
            }
            var timer = Stopwatch.StartNew();
            WriteLine("Running methods on one thread.");
            for (int i = 0; i < dataItems.Length; i++)
            {
                resultItems[i] = Math.Pow(dataItems[i], 2);
            }
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            for (int degree = 1; degree <= 8; degree++)
            {
                ParallelOptions options = new ParallelOptions()
                    { MaxDegreeOfParallelism = degree };
                timer = Stopwatch.StartNew();
                WriteLine($"Running methods on {degree} thread.");
                Parallel.For(0, dataItems.Length, options, (index) => {
                    resultItems[index] = Math.Pow(dataItems[index], 2);
                });
                WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            }
        }

        static void Step3()
        {
            var timer = Stopwatch.StartNew();
            ParallelLoopResult res = Parallel.For(0, 100,
                (int index, ParallelLoopState loopState) => {
                // calculate the square of the index
                double sqr = Math.Pow(index, 2);
                // if the square value is > 100 then break
                if (sqr > 100) {
                    Console.WriteLine("Breaking on index {0}", index);
                    loopState.Break();
                }
                else {
                    // write out the value
                    Console.WriteLine("Square value of {0} is {1}",
                        index, sqr);
                }
            });
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            WriteLine($"IsCompleted = {res.IsCompleted}");
            WriteLine($"LowestBreakIteration = {res.LowestBreakIteration}");
        }

        static void Pause()
        {
            WriteLine("Press ENTER to continue.");
            ReadLine();
        }

        static void Main(string[] args)
        {
            WriteLine("=== Step 1: array size 100, x1000, x1000000 ===");
            Step1(100);
            Step1(100000);
            Step1(100000000);
            Pause();

            WriteLine("=== Step 2: MaxDegreeOfParallelism 1..8 ===");
            Step2();
            Pause();

            WriteLine("=== Step 3: Parallel.For with Break ===");
            Step3();
            WriteLine("Press enter to finish");
            ReadLine();
        }
    }
}
