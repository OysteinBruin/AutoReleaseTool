using System;
using System.Threading;

namespace AutoReleaseTool
{
    class Program
    {
        private static readonly int _consoleSleepTimeMs = 8000;

        static void Main(string[] args)
        {
            Console.WriteLine("\n\nAutoReleaseTool: starting...\n");

            //
            if (args.Length == 3)
            {
                try
                {
                    Console.WriteLine("--- Input arguments:");
                    foreach (var item in args)
                    {
                        Console.WriteLine($" - {item}");
                    }

                    INugetPackageCreator nugetPackackeCreator = new NugetPackageCreator(args[0], args[1], args[2]);
                    ReleaseCreator releaseCreator = new ReleaseCreator(nugetPackackeCreator);
                    string releaseResult = releaseCreator.Execute();

                    if (releaseResult != String.Empty)
                    {
                        Console.WriteLine("\n\n" + releaseResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n\nException: AutoReleaseFailed failed. Exception type: message: {ex.GetType()} {ex.Message} ");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"\tInner exception:  {ex.InnerException.Message} ");
                    }
                }
            }
            else
            {
                Console.WriteLine("\n\nAutoRelease initialization failed - invalid arguments\n\tRequired arguments:" +
                    "\n\t1. Path to build drectory [string]" +
                    "\n\t2. Application Id (Name of application) [string]" +
                    "\n\t3. version in format Major.Minor.Patch (e.e 1.1.1) [string]");
            }
            Console.WriteLine($"\n\nAutoReleaseTool: finished - closing in {Program._consoleSleepTimeMs/1000} seconds ...");
            Thread.Sleep(_consoleSleepTimeMs);
        }
    }
}
