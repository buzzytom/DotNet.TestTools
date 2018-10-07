using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualStudio.TestTools.Projects
{
    public static class TestChangeDetectorHelper
    {
        public static IEnumerable<Project> GetAffectedTestProjects(IEnumerable<Project> testProjects, Uri[] changes)
        {
            IEnumerable<Project> flattened = GetAllProjectsFlat(testProjects);

            // Detect which projects have the file changes
            HashSet<Project> changed = new HashSet<Project>();
            foreach (Project project in flattened)
            {
                if (changes.Any(x => project.FolderUri.IsBaseOf(x)))
                    changed.Add(project);
            }

            // Filter the test affected test projects
            return testProjects
                .Where(x => x.IsAffected(changed))
                .ToArray();
        }

        private static IEnumerable<Project> GetAllProjectsFlat(this IEnumerable<Project> projects)
        {
            HashSet<Project> mapped = new HashSet<Project>();
            foreach (Project project in projects)
                mapped.AddProject(project);
            return mapped;
        }

        private static void AddProject(this HashSet<Project> projects, Project project)
        {
            if (!projects.Contains(project))
            {
                projects.Add(project);
                foreach (Project dependency in project.Dependencies)
                    projects.AddProject(dependency);
            }
        }

        private static bool IsAffected(this Project project, HashSet<Project> changedProjects)
        {
            if (changedProjects.Contains(project))
                return true;

            foreach (Project dependency in project.Dependencies)
            {
                bool affected = dependency.IsAffected(changedProjects);
                if (affected)
                    return true;
            }

            return false;
        }
    }
}
