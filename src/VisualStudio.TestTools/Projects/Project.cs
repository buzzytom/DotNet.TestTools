using System;
using System.Collections.Generic;

namespace DotNet.TestTools.Projects
{
    public class Project
    {
        public Project()
        {
            Dependencies = new List<Project>();
            KnownDependants = new List<Project>();
        }

        public Uri FolderUri { set; get; }

        public Uri Uri { set; get; }

        public Uri BinaryUri { set; get; }

        public string TargetFramework { set; get; }

        public IEnumerable<Project> Dependencies { set; get; }

        public IEnumerable<Project> KnownDependants { set; get; }
    }
}
