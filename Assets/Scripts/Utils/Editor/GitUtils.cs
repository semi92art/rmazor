using System.Diagnostics;

namespace Utils.Editor
{
    public static class GitUtils
    {
        private const string DefaultGitDirectory = "./";
        
        public static string RunGitCommand(string _GitCommand, string _GitDirectory = @".\")
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("git", $"-C {_GitDirectory} {_GitCommand}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        
            var process = new Process {StartInfo = processInfo};

            try {
                process.Start();
            }
            catch (System.Exception) {
                Dbg.LogError("Git is not set-up correctly, required to be on PATH, and to be a git project.");
                throw;
            }

            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();
            process.Close();
        
            if (output.Contains("fatal"))
            {
                string message = "Command: git " + _GitCommand + " Failed\n" + output + errorOutput;
                throw new System.Exception(message);
            }
            if (errorOutput != "") 
                Dbg.Log("Git Message: " + errorOutput);
            return output;
        }
    }
}