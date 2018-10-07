using System;
using System.Collections.Generic;
using System.IO;
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

            // Read the configuration
            IDictionary<string, string> configuration = ReadConfiguration(root, "TargetFramework");

            // Read the target framework
            if (!configuration.TryGetValue("TargetFramework", out string targetFramework))
                throw new Exception($"The project file {uri} does not define a TargetFramework.");

            // Read the output type
            OutputType type = DecodeOutputType(configuration);

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
                FolderUri = new Uri(uri, "./"),
                Uri = uri,
                BinaryUri = GetOutputBinaryUri(uri, targetFramework, type),
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

        private static Uri CreateReferenceUri(Uri current, string reference)
        {
            if (Uri.TryCreate(reference, UriKind.Absolute, out Uri absolute))
                return absolute;
            if (Uri.TryCreate(current, reference, out Uri relative))
                return relative;
            throw new Exception($"Could not resolve a uri for {reference}.");
        }

        private IDictionary<string, string> ReadConfiguration(XmlNode root, params string[] identifiers)
        {
            return root.ChildNodes
                .Where(x => x.Name == "PropertyGroup")
                .SelectMany(x => x.ChildNodes)
                .Where(x => identifiers.Contains(x.Name))
                .ToDictionary(x => x.Name, x => x.InnerText);
        }

        private Uri GetOutputBinaryUri(Uri current, string targetFramework, OutputType type)
        {
            string name = Path.GetFileNameWithoutExtension(current.LocalPath);
            string extension = GetOutputExtension(type);
            return new Uri(current, $"./Debug/{targetFramework}/{name}.{extension}");
        }

        private string GetOutputExtension(OutputType type)
        {
            switch (type)
            {
                case OutputType.Executable:
                    return "exe";
                case OutputType.Dll:
                    return "dll";
                default:
                    throw new NotSupportedException($"The output type of {type} is not known.");
            }
        }

        private OutputType DecodeOutputType(IDictionary<string, string> configuration)
        {
            if (!configuration.TryGetValue("OutputType", out string type))
                return OutputType.Dll;

            switch (type)
            {
                case "Exe":
                    return OutputType.Executable;
                default:
                    throw new NotSupportedException($"The output type of {type} is not known.");
            }
        }

        private enum OutputType
        {
            Dll = 0,
            Executable = 1
        }
    }
}
