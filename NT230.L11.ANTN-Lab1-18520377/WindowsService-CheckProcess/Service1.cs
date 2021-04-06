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

namespace WindowsService_CheckProcess
{
    public partial class Service1 : ServiceBase
    {
        String processName = "notepad";
        Timer timer1 = new Timer(); //Check process running/stop
        Timer timer2 = new Timer(); //Running process as cycle
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //String processName = args[1];

            WriteToFile(DateTime.Now + "Service started");
            timer1.Elapsed += new ElapsedEventHandler(OnElapsedTime1);
            timer1.Interval = 1000; //check after every 1s
            timer1.Enabled = true;
            timer2.Elapsed += new ElapsedEventHandler(OnElapsedTime2);
            timer2.Interval = 5000; //cycle running/stop 5s
            timer2.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile(DateTime.Now + "Service stopped");
        }

        private void OnElapsedTime1(object source, ElapsedEventArgs e)
        {
            if (isProcessRunning(processName))
            {
                //write to file log
                WriteToFile(DateTime.Now + " Process is running...");
            }
            else
            {
                //write to log
                WriteToFile(DateTime.Now + " Process is not running...");
            }
        }

        private void OnElapsedTime2(object source, ElapsedEventArgs e)
        {
            if (isProcessRunning(processName))
            {
                //get all id and stop it
                Process[] listofProcess = Process.GetProcessesByName("notepad");
                int n = listofProcess.Length;
                while (n > 0)
                {
                    listofProcess[n - 1].Kill();
                    listofProcess[n - 1].WaitForExit();
                    WriteToFile(DateTime.Now + " Process stopped!");
                }
            }
            else
            {
                //just open process
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.FileName = "notepad";
                    myProcess.Start();
                    //check and write to file
                    if (myProcess.Id != 0)
                        WriteToFile(DateTime.Now + " Process started");
                    else
                        WriteToFile(DateTime.Now + " Has some problem when try start process");
                }
            }
        }
        private bool isProcessRunning(String processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);
            if (arrayProcess.Length != 0)
                return true;
            else
                return false;
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs"; //get path
            if (!Directory.Exists(path))
            {
                //Check if path not exists, then create this directory
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ProcessNotePad_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt"; //create file log with pattern
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
    }
}
