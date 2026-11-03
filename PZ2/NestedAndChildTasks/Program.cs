using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Console;

namespace NestedAndChildTasks
{
    class Program
    {
        static void Step1()
        {
            var outer = Task.Factory.StartNew(() =>
            {
                WriteLine("Outer task starting...");
                var inner = Task.Factory.StartNew(() =>
                {
                    WriteLine("Inner task starting...");
                    Thread.Sleep(2000);
                    WriteLine("Inner task finished.");
                });
            });
            outer.Wait();
            WriteLine("Outer task finished.");
            WriteLine("Press ENTER to end.");
            ReadLine();
        }

        static void Step2()
        {
            var outer = Task.Factory.StartNew(() =>
            {
                WriteLine("Outer task starting...");
                var inner = Task.Factory.StartNew(() =>
                {
                    WriteLine("Inner task starting...");
                    Thread.Sleep(2000);
                    WriteLine("Inner task finished.");
                }, TaskCreationOptions.AttachedToParent);
            });
            outer.Wait();
            WriteLine("Outer task finished.");
            WriteLine("Press ENTER to end.");
            ReadLine();
        }

        static void Main(string[] args)
        {
            WriteLine("=== Step 1: nested task ===");
            Step1();

            WriteLine();
            WriteLine("=== Step 2: AttachedToParent ===");
            Step2();
        }
    }
}