using System.Threading.Tasks;
using VisualStudio.TestTools.Projects;

namespace VisualStudio.TestTools.Testing
{
    public interface ITestRunner
    {
        Task<bool> Build(Project project);
        Task<bool> RunTests(Project project);
    }
}