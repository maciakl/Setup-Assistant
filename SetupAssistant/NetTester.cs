using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;

using wInformation;

namespace SetupAssistant
{
    public partial class NetTester : Form
    {
        
        public NetTester()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void runProcess(string command, string args)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = command;
            processInfo.Arguments = args;
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.CreateNoWindow = true;

            int counter = 0;

            Process p = Process.Start(processInfo);

            System.IO.StreamReader reader = p.StandardOutput;

            
            string output = "";

            Stopwatch timer = new Stopwatch();
            timer.Start();

            try
            {
                while ((output = reader.ReadLine()) != null)
                {
                    counter++;

                    if (output != "")
                        textBox1.AppendText(output + "\r\n");

                    if (timer.ElapsedMilliseconds > 100000)
                    {
                        textBox1.AppendText("\r\nFAILED: Request took too long. Test aborted. Try local server.");
                        break;
                    }

                }
            }
            catch (IOException e)
            { MessageBox.Show(e.Message);  }

            timer.Stop();
            reader.Close();

        }

                             
        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = String.Empty;
            this.textBox1.Clear();
            this.Refresh();

            this.textBox1.AppendText("Pinging host...\r\n");

            this.runProcess("ping", " -w 350 " + this.addr.Text);

        }

        private void pingTestClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = String.Empty;
            this.textBox1.Clear();
            this.Refresh();

            this.textBox1.AppendText("Looking up host...\r\n");

            this.runProcess("nslookup", this.addr.Text);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = String.Empty;
            this.textBox1.Clear();
            this.Refresh();

            this.textBox1.AppendText("Tracing route...\r\n");
            this.textBox1.AppendText("This will take a long time...\r\n");
            this.textBox1.AppendText("Please be patient...\r\n");
                        
            this.runProcess("tracert", " -w 350 " + this.addr.Text);
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            foreach (NetworkInterface networkCard in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkCard.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation gatewayAddr in networkCard.GetIPProperties().GatewayAddresses)
                    {
                        this.addr.Text = gatewayAddr.Address.ToString();
                    }

                    break;
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.addr.Text = "127.0.0.1";
        }
    }
}
