using System;
using System.Linq;

namespace VisualStudio.TestTools.ConsoleApp
{
    public class Configuration
    {
        public Uri[] GetTestProjectUris(Uri workingDirectory)
        {
            return TestProjects
                .Select(x => new Uri(workingDirectory, x))
                .ToArray();
        }

        // ----- Properties ----- //

        public string ChangesCommand { set; get; }

        public string[] TestProjects { set; get; }
    }
}
