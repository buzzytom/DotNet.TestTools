using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VisualStudio.TestTools
{
    public static class ProcessHelper
    {
        public static Task<int> Run(Uri workingDirectory, string arguments)
        {
            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    WorkingDirectory = workingDirectory.LocalPath,
                    Arguments = $"/C {arguments}",
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                completion.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return completion.Task;
        }
    }
}
