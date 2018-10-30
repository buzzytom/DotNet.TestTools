using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DotNet.TestTools
{
    public static class ProcessHelper
    {
        public static Task<int> Run(Uri workingDirectory, string arguments)
        {
            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();

            string filename = "/bin/bash";
            string processArguments = "-c \"{0}\"";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filename = "cmd.exe";
                processArguments = "/C {0}";
            }

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = filename,
                    WorkingDirectory = workingDirectory.LocalPath,
                    Arguments = string.Format(processArguments, arguments),
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
