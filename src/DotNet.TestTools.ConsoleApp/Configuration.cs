using LibGit2Sharp;
using System;
using System.Linq;

namespace DotNet.TestTools.ConsoleApp
{
    public class Configuration
    {
        public Uri[] GetTestProjectUris(Uri workingDirectory)
        {
            return TestProjects
                .Select(x => new Uri(workingDirectory, x))
                .ToArray();
        }

        public Uri[] GetChanges(Uri workingDirectory)
        {
            workingDirectory = new Uri(workingDirectory, RepositoryRelativePath);

            using (Repository repository = new Repository(workingDirectory.LocalPath))
            {
                return repository.Diff
                    .Compare<TreeChanges>(repository.Head.Tip.Tree, DiffTargets.Index)
                    .SelectMany(x => new[] { x.Path, x.OldPath })
                    .Distinct()
                    .Select(x => new Uri(workingDirectory, new Uri(x, UriKind.Relative)))
                    .ToArray();
            }
        }

        // ----- Properties ----- //

        public string RepositoryRelativePath { set; get; }

        public string[] TestProjects { set; get; }
    }
}
