using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VisualStudio.TestTools.Projects;

namespace VisualStudio.TestTools.ConsoleApp
{
    public static class Program
    {
        private static Stopwatch stopwatch = new Stopwatch();

        public static void Main(string[] args)
        {
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

            // Read the projects structures
            Console.WriteLine($"Reading {testProjectUris.Length} test project{testProjectUris.Length.GetPluralisation()}...");
            stopwatch.Start();
            IEnumerable<Project> projects = ReadProjects(testProjectUris);
            stopwatch.Stop();
            Console.WriteLine($"Done!");
            Console.WriteLine($"{stopwatch.Elapsed.Milliseconds}ms");

            // Detect the test projects affected by the changes
            Console.WriteLine();
            Console.WriteLine($"Analyzing test projects affected by the file changes.");
            stopwatch.Restart();
            projects = TestChangeDetectorHelper.GetAffectedTestProjects(projects, changes);
            int count = projects.Count();
            stopwatch.Stop();
            Console.WriteLine($"Done! {count} test project{count.GetPluralisation()} affected.");
            Console.WriteLine($"{stopwatch.Elapsed.Milliseconds}ms");
        }

        private static IEnumerable<Project> ReadProjects(IEnumerable<Uri> paths)
        {
            IProjectReader reader = new CsProjectReader();
            return paths
                .Select(x => reader.ReadProject(x))
                .ToArray();
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
