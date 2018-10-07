using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNet.TestTools.Projects;
using DotNet.TestTools.Testing;

namespace DotNet.TestTools.ConsoleApp
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Run(args)
                    .GetAwaiter()
                    .GetResult();

                stopwatch.Stop();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("All Tasks Complete.");
                Console.WriteLine($"{stopwatch.Elapsed.Milliseconds}ms");

                return 0;
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(exception.Message);
                return 1;
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static async Task Run(string[] args)
        {
            // Get the configuration path
            string configurationPath = args.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(configurationPath))
                throw new ArgumentException("A configuration path must be specified.");

            // Read the configuration
            string text = File.ReadAllText(configurationPath);
            Configuration configuration = JsonConvert.DeserializeObject<Configuration>(text);

            // Decode the configuration values
            Uri workingDirectory = new Uri(Environment.CurrentDirectory, UriKind.Absolute);
            Uri[] testProjectUris = configuration.GetTestProjectUris(workingDirectory);
            Uri[] changes = configuration.GetChanges(workingDirectory);

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
        }

        private static Project[] ReadProjects(this Uri[] paths)
        {
            Console.WriteLine($"Reading {paths.Length} test project{paths.Length.GetPluralisation()}...");
            Stopwatch stopwatch = Stopwatch.StartNew();

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
            Stopwatch stopwatch = Stopwatch.StartNew();

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
            Stopwatch stopwatch = Stopwatch.StartNew();
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
