using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using static System.Console;

namespace Partitioning
{
    class Program
    {
        delegate void ProcessValue(int value);
        static double[] resultData = new double[10000000];

        static void Step1()
        {
            var timer = Stopwatch.StartNew();
            WriteLine("Running Parallel.For with lambda expression.");
            // perform the parallel loop
            Parallel.For(0, resultData.Length, (int index) => {
                // compute the result for the current index
                resultData[index] = Math.Pow(index, 2);
            });
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");

            // perform the loop again, but make the delegate explicit
            timer = Stopwatch.StartNew();
            WriteLine("Running Parallel.For with explicit delegate.");
            Parallel.For(0, resultData.Length, delegate (int index) {
                resultData[index] = Math.Pow((double)index, 2);
            });
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");

            // perform the loop once more, but this time using
            // a declared delegate and action
            timer = Stopwatch.StartNew();
            WriteLine("Running Parallel.For with delegate and Action.");
            ProcessValue pdel = new ProcessValue(computeResultValue);
            Action<int> paction = new Action<int>(pdel);
            Parallel.For(0, resultData.Length, paction);
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
        }

        static void Step2()
        {
            // create the results array
            double[] resultData = new double[10000000];
            // created a partioner that will chunk the data
            OrderablePartitioner<Tuple<int, int>> chunkPart
                = Partitioner.Create(0, resultData.Length, 10000);

            var timer = Stopwatch.StartNew();
            WriteLine("Running Parallel.ForEach in chunks of 10,000.");

            // perform the loop in chunks
            Parallel.ForEach(chunkPart, chunkRange =>
            {
                // iterate through all of the values in the chunk range
                for (int i = chunkRange.Item1; i < chunkRange.Item2; i++)
                {
                    resultData[i] = Math.Pow(i, 2);
                }
            });
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
        }

        static void Main(string[] args)
        {
            Step1();
            Step2();

            // wait for input before exiting
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }

        static void computeResultValue(int indexValue)
        {
            resultData[indexValue] = Math.Pow(indexValue, 2);
        }
    }
}
