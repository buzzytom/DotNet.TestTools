using System;

namespace VisualStudio.TestTools.Projects
{
    public interface IProjectReader
    {
        Project ReadProject(Uri uri);
    }
}