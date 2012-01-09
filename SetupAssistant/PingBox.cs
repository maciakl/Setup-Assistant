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


namespace SetupAssistant
{
    public partial class PingBox : Form
    {
        int pingNumber = 8;
        int timeout = 120; //1000;

        // Create a buffer of 32 bytes of data to be transmitted.
        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        string address = "google.com";


        public PingBox()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
        }

        public int PingNumber { get { return pingNumber; } set { pingNumber = value; } }
        public int Timeout { get { return timeout; } set { timeout = value; } }
        public string Data { get { return data; } set { data = value; } }
        public string Address { get { return address; } set { address = value; } }
                
        private void button1_Click(object sender, EventArgs e)
        {
            
            Ping pingSender = new Ping ();
            PingOptions options = new PingOptions ();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

                      

            if (this.addr.Text != String.Empty)
                address = this.addr.Text;
            

            byte[] buffer = Encoding.ASCII.GetBytes (data);
            
            this.textBox1.Text = "";

            this.textBox1.AppendText("Ping test initiated at: " + DateTime.Now + "\r\n");
            this.textBox1.AppendText("Attempting to connect to: " + address + "\r\n");

            try
            {
                int successful = 0;

                for (int i = 0; i < pingNumber; i++)
                {
                                    
                    PingReply reply = pingSender.Send(address, timeout, buffer, options);

                    if (reply.Status == IPStatus.Success)
                    {

                        this.textBox1.AppendText("Reply from " + reply.Address.ToString() + ": bytes=" + reply.Buffer.Length
                                                    + " time=" + reply.RoundtripTime + " TTL=" + reply.Options.Ttl + "\r\n");
                        successful++;

                    }
                    else
                    {
                        this.textBox1.AppendText("Request timed out.\r\n");
                    }

                }
                int lost = pingNumber - successful;
                double pct = ((double)lost/(double)pingNumber)*100;

                this.textBox1.AppendText("Packets sent: " + pingNumber + " Packets received: " + successful + " Packets lost: " + lost + " ");
                this.textBox1.AppendText(" ( " + pct + "% )\r\n"); 

            }
            catch (PingException pe)
            {
                this.textBox1.AppendText(pe.InnerException.Message);
            }

        }

        private void pingTestClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
