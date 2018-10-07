using System.Threading.Tasks;
using DotNet.TestTools.Projects;

namespace DotNet.TestTools.Testing
{
    public interface ITestRunner
    {
        Task<bool> RunTests(Project project);
    }
}