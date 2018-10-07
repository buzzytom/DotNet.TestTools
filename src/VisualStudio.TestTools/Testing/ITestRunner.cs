using System.Threading.Tasks;
using VisualStudio.TestTools.Projects;

namespace VisualStudio.TestTools.Testing
{
    public interface ITestRunner
    {
        Task<bool> RunTests(Project project);
    }
}