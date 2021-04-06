using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace SimpleWindowsService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs"; //get path
            if (!Directory.Exists(path))
            {
                //Check if path not exists, then create this directory
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt"; //create file log with pattern
            if (!File.Exists(filepath))
            {
                //if this file not exists, then create this file.
                using (StreamWriter sw = File.CreateText(filepath))
                    sw.WriteLine(Message);
            }
            else
            {
                //because this file exists, i only write Message to this file
                using (StreamWriter sw = File.AppendText(filepath))
                    sw.WriteLine(Message);
            }
        }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in miliseconds
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
    }
}
