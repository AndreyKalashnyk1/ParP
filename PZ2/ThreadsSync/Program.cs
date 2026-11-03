using System;
using System.Threading;

namespace ThreadsSync
{
    public class MyTheard
    {
        public void ThreadNumbers()
        {
            // Информація про потік
            Console.WriteLine("{0} потік використовує метод ThreadNumbers", Thread.CurrentThread.Name);
            // Виводимо числа
            Console.Write("Числа: ");
            for (int i = 0; i < 10; i++)
            {
                Random rand = new Random();
                Thread.Sleep(1000 * rand.Next(5));
                Console.Write(i + ", ");
            }
            Console.WriteLine();
        }
    }

    public class MyTheardLock
    {
        private object threadLock = new object();

        public void ThreadNumbers()
        {
            // Використовуємо маркер блокування
            lock (threadLock)
            {
                // Информація про потік
                Console.WriteLine("{0} потік використовує метод ThreadNumbers", Thread.CurrentThread.Name);
                // Виводимо числа
                Console.Write("Числа: ");
                for (int i = 0; i < 10; i++)
                {
                    Random rand = new Random();
                    Thread.Sleep(1000 * rand.Next(5));
                    Console.Write(i + ", ");
                }
                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Run(ThreadStart method)
        {
            // Створюємо 10 потоків
            Thread[] threads = new Thread[10];

            for (int i = 0; i < 10; i++)
            {
                threads[i] = new Thread(method);
                threads[i].Name = string.Format("Работает поток: #{0}", i);
            }

            // Запускаємо всі потоки
            foreach (Thread t in threads)
                t.Start();

            foreach (Thread t in threads)
                t.Join();
        }

        static void Main()
        {
            Console.WriteLine("=== Step 1: without lock ===");
            MyTheard mt = new MyTheard();
            Run(mt.ThreadNumbers);
            Console.WriteLine("Press ENTER to continue.");
            Console.ReadLine();

            Console.WriteLine("=== Step 2: lock ===");
            MyTheardLock mtl = new MyTheardLock();
            Run(mtl.ThreadNumbers);
            Console.ReadLine();
        }
    }
}