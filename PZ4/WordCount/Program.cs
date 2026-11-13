using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using static System.Console;

namespace WordCount
{
    class Page
    {
        public string Text { get; }
        public bool IsPoisonPill { get; }

        public Page(string text, bool isPoisonPill = false)
        {
            Text = text;
            IsPoisonPill = isPoisonPill;
        }
    }

    class Program
    {
        const string FileName = "text.txt";
        const int LinesPerPage = 10;
        const int Runs = 5;
        static char[] separators = { ' ', '\t', '\r', '\n', '.', ',',
            ';', ':', '!', '?', '"', '(', ')', '-' };

        static BlockingCollection<Page> queue;
        static Dictionary<string, int> counts;
        static object countsLock = new object();
        static ConcurrentDictionary<string, int> ccounts;

        static IEnumerable<Page> Pages()
        {
            List<string> lines = new List<string>();
            foreach (string line in File.ReadLines(FileName))
            {
                lines.Add(line);
                if (lines.Count == LinesPerPage)
                {
                    yield return new Page(string.Join("\n", lines));
                    lines.Clear();
                }
            }
            if (lines.Count > 0)
                yield return new Page(string.Join("\n", lines));
        }

        static string[] Words(string text)
        {
            return text.ToLower().Split(separators,
                StringSplitOptions.RemoveEmptyEntries);
        }

        static void Parser()
        {
            foreach (Page page in Pages())
                queue.Add(page);
        }

        static void Counter(int variant)
        {
            Dictionary<string, int> localCounts =
                new Dictionary<string, int>();
            while (true)
            {
                Page page = queue.Take();
                if (page.IsPoisonPill)
                    break;

                foreach (string word in Words(page.Text))
                {
                    if (variant == 1)
                        CountWordLock(word);
                    else if (variant == 2)
                        CountWordAtomic(word);
                    else
                    {
                        int count;
                        localCounts.TryGetValue(word, out count);
                        localCounts[word] = count + 1;
                    }
                }
            }
            if (variant == 3)
                MergeCounts(localCounts);
        }

        static void CountWordLock(string word)
        {
            lock (countsLock)
            {
                int currentCount;
                if (!counts.TryGetValue(word, out currentCount))
                    counts[word] = 1;
                else
                    counts[word] = currentCount + 1;
            }
        }

        static void CountWordAtomic(string word)
        {
            while (true)
            {
                int currentCount;
                if (!ccounts.TryGetValue(word, out currentCount))
                {
                    if (ccounts.TryAdd(word, 1))
                        break;
                }
                else if (ccounts.TryUpdate(word, currentCount + 1,
                    currentCount))
                    break;
            }
        }

        static void MergeCounts(Dictionary<string, int> localCounts)
        {
            foreach (KeyValuePair<string, int> e in localCounts)
            {
                string word = e.Key;
                int count = e.Value;
                while (true)
                {
                    int currentCount;
                    if (!ccounts.TryGetValue(word, out currentCount))
                    {
                        if (ccounts.TryAdd(word, count))
                            break;
                    }
                    else if (ccounts.TryUpdate(word, currentCount + count,
                        currentCount))
                        break;
                }
            }
        }

        static double Sequential()
        {
            var timer = Stopwatch.StartNew();
            counts = new Dictionary<string, int>();
            foreach (Page page in Pages())
                foreach (string word in Words(page.Text))
                {
                    int count;
                    counts.TryGetValue(word, out count);
                    counts[word] = count + 1;
                }
            return timer.Elapsed.TotalMilliseconds;
        }

        static double RunParallel(int variant, int numCounters)
        {
            queue = new BlockingCollection<Page>(
                new ConcurrentQueue<Page>(), 100);
            counts = new Dictionary<string, int>();
            ccounts = new ConcurrentDictionary<string, int>();

            var timer = Stopwatch.StartNew();
            Task[] counters = new Task[numCounters];
            for (int i = 0; i < numCounters; ++i)
                counters[i] = Task.Factory.StartNew(() => Counter(variant),
                    TaskCreationOptions.LongRunning);
            Task parser = Task.Factory.StartNew(Parser,
                TaskCreationOptions.LongRunning);
            parser.Wait();
            for (int i = 0; i < numCounters; ++i)
                queue.Add(new Page(null, true));
            Task.WaitAll(counters);
            return timer.Elapsed.TotalMilliseconds;
        }

        static long TotalWords(int variant)
        {
            return variant == 1 ? counts.Values.Sum(v => (long)v)
                                : ccounts.Values.Sum(v => (long)v);
        }

        static int UniqueWords(int variant)
        {
            return variant == 1 ? counts.Count : ccounts.Count;
        }

        static void Main(string[] args)
        {
            string[] names = { "", "Dictionary + lock",
                "ConcurrentDictionary (TryAdd / TryUpdate)",
                "local counts + merge" };

            WriteLine($"File: {FileName}, " +
                $"size: {new FileInfo(FileName).Length:#,##0} bytes");

            double seq = double.MaxValue;
            for (int r = 0; r < Runs; r++)
                seq = Math.Min(seq, Sequential());
            WriteLine($"Sequential: time {seq:0.0}ms  " +
                $"words: {counts.Values.Sum(v => (long)v):#,##0}  " +
                $"unique: {counts.Count}");

            for (int variant = 1; variant <= 3; variant++)
            {
                WriteLine($"--- Variant {variant}: {names[variant]} ---");
                for (int n = 1; n <= 7; n++)
                {
                    double time = double.MaxValue;
                    for (int r = 0; r < Runs; r++)
                        time = Math.Min(time, RunParallel(variant, n));
                    WriteLine($"Consumers: {n}  time: {time,6:0.0}ms  " +
                        $"speedup: {seq / time:0.00}  " +
                        $"words: {TotalWords(variant):#,##0}  " +
                        $"unique: {UniqueWords(variant)}");
                }
            }

            WriteLine("Press enter to finish");
            ReadLine();
        }
    }
}