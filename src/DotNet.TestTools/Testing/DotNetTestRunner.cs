using System.Threading.Tasks;
using DotNet.TestTools.Projects;

namespace DotNet.TestTools.Testing
{
    public class DotNetTestRunner : ITestRunner
    {
        public async Task<bool> RunTests(Project project)
        {
            return await ProcessHelper.Run(project.FolderUri, "dotnet test") == 0;
        }
    }
}
