using System;

namespace DotNet.TestTools.Projects
{
    public interface IProjectReader
    {
        Project ReadProject(Uri uri);
    }
}