using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using static System.Console;

namespace BankBalances
{
    class Transaction { public int Amount { get; set; } }

    class Example_01
    {
        static Transaction[] sourceData;
        // set the number of items created
        static int itemsPerMonth = 100000;

        static void CreateData()
        {
            // create a random number generator
            Random rnd = new Random();

            // create the source data
            sourceData = new Transaction[12 * itemsPerMonth];
            for (int i = 0; i < 12 * itemsPerMonth; i++)
            {
                sourceData[i] = new Transaction()
                    { Amount = rnd.Next(-400, 500) };
            }
        }

        static int[] Sequential()
        {
            int[] monthlyBalances = new int[12];

            for (int currentMonth = 0; currentMonth < 12; currentMonth++)
            {
                int start = currentMonth * itemsPerMonth;
                for (int index = start; index < start + itemsPerMonth; index++)
                {
                    monthlyBalances[currentMonth] +=
                        sourceData[index].Amount;
                }
                // add the previous month's balance
                if (currentMonth > 0) monthlyBalances[currentMonth] +=
                    monthlyBalances[currentMonth - 1];
            }
            return monthlyBalances;
        }

        static int[] ParallelNoSync()
        {
            // create the results array
            int[] monthlyBalances = new int[12];

            for (int currentMonth = 0; currentMonth < 12; currentMonth++)
            {
                // perform the parallel loop on the current month's data
                Parallel.For(
                    currentMonth * itemsPerMonth, //початковий індекс
                    (currentMonth + 1) * itemsPerMonth, //кінцевий індекс
                    new ParallelOptions(),
                    () => 0,
                    (index, loopstate, tlsBalance) => {
                        return tlsBalance += sourceData[index].Amount;
                    },
                    tlsBalance => monthlyBalances[currentMonth] += tlsBalance);
                // end of parallel for
                // add the previous month's balance
                if (currentMonth > 0) monthlyBalances[currentMonth] +=
                    monthlyBalances[currentMonth - 1];
            }
            return monthlyBalances;
        }

        static int[] ParallelInterlocked()
        {
            int[] monthlyBalances = new int[12];

            for (int currentMonth = 0; currentMonth < 12; currentMonth++)
            {
                Parallel.For(
                    currentMonth * itemsPerMonth,
                    (currentMonth + 1) * itemsPerMonth,
                    new ParallelOptions(),
                    () => 0,
                    (index, loopstate, tlsBalance) => {
                        return tlsBalance += sourceData[index].Amount;
                    },
                    tlsBalance => Interlocked.Add(
                        ref monthlyBalances[currentMonth], tlsBalance));
                if (currentMonth > 0) monthlyBalances[currentMonth] +=
                    monthlyBalances[currentMonth - 1];
            }
            return monthlyBalances;
        }

        static int[] Run(string name, Func<int[]> method, int[] reference)
        {
            WriteLine($"--- {name} ---");
            var timer = Stopwatch.StartNew();
            int[] monthlyBalances = method();
            WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");
            for (int i = 0; i < monthlyBalances.Length; i++) {
                Console.WriteLine("Month {0} - Balance: {1}",
                    i, monthlyBalances[i]); }
            if (reference != null)
            {
                bool equal = monthlyBalances.SequenceEqual(reference);
                WriteLine($"Equal to sequential: {equal}");
            }
            return monthlyBalances;
        }

        static void Main(string[] args)
        {
            CreateData();
            int[] sequential = Run("1: sequential", Sequential, null);
            Run("2: no synchronization", ParallelNoSync, sequential);
            Run("3: Interlocked.Add", ParallelInterlocked, sequential);

            // wait for input before exiting
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }
    }
}
