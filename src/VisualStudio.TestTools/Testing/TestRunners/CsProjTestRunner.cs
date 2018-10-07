using System.Threading.Tasks;
using VisualStudio.TestTools.Projects;

namespace VisualStudio.TestTools.Testing
{
    public class CsProjTestRunner : ITestRunner
    {
        public async Task<bool> RunTests(Project project)
        {
            return await ProcessHelper.Run(project.FolderUri, "dotnet test") == 0;
        }
    }
}
