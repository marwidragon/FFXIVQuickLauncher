using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace XIVLauncher.RunAs
{
    public class Program
    {
        public static string Name => "XIVLauncher.RunAs.exe";

        public static void Main(string[] args)
        {
            while (true)
            {
                var input = Console.ReadLine();

                if (File.Exists(input))
                {
                    var p = new Process();

                    p.StartInfo.FileName = input;
                    p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(input);
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                    p.WaitForInputIdle(10 * 1000);
                }

                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
        }
    }
}
