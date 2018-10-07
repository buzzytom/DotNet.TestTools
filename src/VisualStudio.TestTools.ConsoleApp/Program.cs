using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisualStudio.TestTools.Projects;
using VisualStudio.TestTools.Testing;

namespace VisualStudio.TestTools.ConsoleApp
{
    public static class Program
    {
        private static Stopwatch stopwatch = new Stopwatch();
        private static Stopwatch stopwatchTotal = new Stopwatch();

        public static int Main(string[] args)
        {
            try
            {
                Run(args)
                    .GetAwaiter()
                    .GetResult();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public static async Task Run(string[] args)
        {
            stopwatchTotal.Start();

            Uri[] testProjectUris = new Uri[]
            {
            };

            Uri[] changes = new Uri[]
            {
            };

            if (testProjectUris.Length == 0)
            {
                Console.WriteLine("No test projects.");
                return;
            }

            if (changes.Length == 0)
            {
                Console.WriteLine("No changes to test.");
                return;
            }

            // Get the affected projects
            Project[] projects = testProjectUris
                .ReadProjects()
                .GetAffectedByChanges(changes);

            // Build and run tests
            ITestRunner runner = new DotNetTestRunner();
            foreach (Project project in projects)
            {
                Console.WriteLine();
                Console.WriteLine($"----- {project.GetShortName()} -----");
                await project.Test(runner);
            }

            stopwatchTotal.Stop();
            Console.WriteLine();
            Console.WriteLine("All Tasks Complete.");
            Console.WriteLine($"{stopwatchTotal.Elapsed.Milliseconds}ms");
        }

        private static Project[] ReadProjects(this Uri[] paths)
        {
            Console.WriteLine($"Reading {paths.Length} test project{paths.Length.GetPluralisation()}...");
            stopwatch.Restart();

            IProjectReader reader = new CsProjectReader();
            Project[] projects = paths
                .Select(x => reader.ReadProject(x))
                .ToArray();

            stopwatch.Stop();
            Console.WriteLine($"Done!");
            Console.WriteLine($"{stopwatch.Elapsed.Milliseconds}ms");

            return projects;
        }

        private static Project[] GetAffectedByChanges(this IEnumerable<Project> projects, Uri[] changes)
        {
            Console.WriteLine();
            Console.WriteLine($"Analyzing test projects affected by the file changes.");
            stopwatch.Restart();

            Project[] changed = TestChangeDetectorHelper
                .GetAffectedTestProjects(projects, changes)
                .ToArray();

            stopwatch.Stop();
            Console.WriteLine($"Done! {changed.Length} test project{changed.Length.GetPluralisation()} affected.");
            Console.WriteLine($"{stopwatch.Elapsed.Milliseconds}ms");

            return changed;
        }

        private static async Task Test(this Project project, ITestRunner runner)
        {
            stopwatch.Restart();
            Console.WriteLine();
            Console.WriteLine($"Running tests...");
            if (!await runner.RunTests(project))
            {
                Console.WriteLine($"One or more tests failed! {project.GetShortName()}");
                throw new Exception($"One or more tests failed! {project.GetShortName()}");
            }
            stopwatch.Stop();
            Console.WriteLine("Done!");
            Console.WriteLine($"{stopwatch.Elapsed.Milliseconds}ms");
        }

        private static string GetShortName(this Project project)
        {
            return Path.GetFileNameWithoutExtension(project.Uri.LocalPath);
        }

        private static string GetPluralisation(this int quantity)
        {
            if (quantity == 1)
                return "";
            else
                return "s";
        }
    }
}
