using System.Threading.Tasks;
using VisualStudio.TestTools.Projects;

namespace VisualStudio.TestTools.Testing
{
    public class CsProjTestRunner : ITestRunner
    {
        public Task<bool> Build(Project project)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RunTests(Project project)
        {
            return Task.FromResult(true);
        }
    }
}
