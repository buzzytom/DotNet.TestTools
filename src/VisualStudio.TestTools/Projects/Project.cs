using System;
using System.Collections.Generic;

namespace VisualStudio.TestTools.Projects
{
    public class Project
    {
        public Uri Uri { set; get; }

        public string TargetFramework { set; get; }

        public IEnumerable<Project> References { set; get; }
    }
}
