using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace VisualStudio.TestTools.Projects
{
    public class CsProjectReader : IProjectReader
    {
        private HashSet<Project> projects = new HashSet<Project>();

        public Project ReadProject(Uri uri)
        {
            // Resolve if the project has already been parsed
            Project existing = projects.FirstOrDefault(x => x.Uri == uri);
            if (existing != null)
                return existing;

            // Load the document
            XmlDocument document = new XmlDocument();
            document.Load(uri.ToString());

            // Read the root node
            XmlNode root = document.ChildNodes.FirstOrDefault(x => x.Name == "Project");
            if (root == null)
                throw new Exception("No root Project element found.");

            // Read the target framework
            string targetFramework = root.ChildNodes
                .Where(x => x.Name == "PropertyGroup")
                .SelectMany(x => x.ChildNodes)
                .FirstOrDefault(x => x.Name == "TargetFramework")?
                .InnerText;

            // Read the project references
            Uri[] references = root.ChildNodes
                .Where(x => x.Name == "ItemGroup")
                .SelectMany(x => x.ChildNodes)
                .Where(x => x.Name == "ProjectReference")
                .Select(x => x.ReadAttribute("Include"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => CreateReferenceUri(uri, x))
                .ToArray();

            // Create and index the new project instance
            Project created = new Project
            {
                Uri = uri,
                TargetFramework = targetFramework,
                Dependencies = references
                    .Select(x => ReadProject(x))
                    .ToArray()
            };
            projects.Add(created);

            // Make sure the created project is a dependant of all its references
            foreach (Project parent in created.Dependencies)
            {
                parent.KnownDependants = parent.KnownDependants
                    .Union(new[] { created })
                    .ToArray();
            }

            return created;
        }

        private Uri CreateReferenceUri(Uri current, string reference)
        {
            if (Uri.TryCreate(reference, UriKind.Absolute, out Uri absolute))
                return absolute;
            if (Uri.TryCreate(current, reference, out Uri relative))
                return relative;
            throw new Exception($"Could not resolve a uri for {reference}.");
        }
    }
}
