using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.ServiceProcess;

using System.IO;


namespace SetupAssistant
{
    class ProcessRunner
    {
        public static string runAndCapture(string command, string args)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = command;
            processInfo.Arguments = args;
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.CreateNoWindow = true;


            Process p = Process.Start(processInfo);
            System.IO.StreamReader reader = p.StandardOutput;
            string output = reader.ReadToEnd();
            reader.Close();

            return output;
        }
                
        public static string runElevatedAndCapture(string command, string args)
        {
            string file = System.Environment.GetEnvironmentVariable("TEMP") + "\\" + Path.GetRandomFileName();

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = command;
            processInfo.Arguments = args + " > " + file;
            processInfo.CreateNoWindow = true;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                  
            Process p = Process.Start(processInfo);
            p.WaitForExit();

            return file;
        }
    }

    



}
