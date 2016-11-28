using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;

namespace unsleepdev
{
    public partial class Daemon : ServiceBase
    {
        private string ConfigFile;
        private List<string> Drives = new List<string>();

        public Daemon()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ConfigFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "unsleepdev.conf");

            if (File.Exists(ConfigFile))
            {
                Drives.AddRange(File.ReadAllLines(ConfigFile));
                bgThread.RunWorkerAsync();
            }
        }

        protected override void OnStop()
        {
            bgThread.CancelAsync();
        }

        private void bgThread_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                foreach (var item in Drives)
                {
                    var drive = item;

                    if (drive.Length >= 1)
                        drive = drive.Remove(1);

                    if (Directory.Exists($"{drive}:\\"))
                    {
                        try
                        {
                            File.Delete($"{drive}:\\unsleepdev.sys");
                            File.WriteAllText($"{drive}:\\unsleepdev.sys", $"UNSLEEPDEV\x00\xDE\xAD\x00\xFA\xCE\x00{DateTime.Now}\x00\xAB\xCD\xEF\x00");
                            File.SetAttributes($"{drive}:\\unsleepdev.sys", FileAttributes.Hidden);
                        }
                        catch (Exception x)
                        {
                            EventLog.WriteEntry($"ERROR TO WRITE\r\nDISK {drive}:\\ IS READ ONLY\r\n{x}", EventLogEntryType.Error);
                        }
                    }
                }

                Thread.Sleep(60000);
            }
        }
    }
}
