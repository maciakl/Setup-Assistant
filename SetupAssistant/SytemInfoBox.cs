using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;
using System.Management;
using System.Diagnostics;

using wInformation;


namespace SetupAssistant
{
    public partial class Info : Form
    {
        public Info(wSystemInformation Info)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;

            this.infobox.AppendText("Info Collected at: \t" + DateTime.Now + "\r\n");

            this.infobox.AppendText("\r\nGENERAL INFORMATION\r\n");
            this.infobox.AppendText("Computer Name: \t" + Info.ComputerName + "\r\n");
            this.infobox.AppendText("Manufacturer: \t" + Info.Manufacturer + "\r\n");
            this.infobox.AppendText("System Model: \t" + Info.Model + "\r\n");

            this.infobox.AppendText("\r\nOPERATING SYSTEM\r\n");
            this.infobox.AppendText("OS Version: \t" + Info.OSVersion + "\r\n");
            this.infobox.AppendText("Service Pack: \t" + Info.ServicePack + "\r\n");
            this.infobox.AppendText("Serial Number: \t" + Info.SerialNumber + "\r\n");
            this.infobox.AppendText("System Drive: \t" + Info.SystemDrive + "\r\n");
            this.infobox.AppendText("Windows Directory: \t" + Info.WindowsDirectory + "\r\n");
            this.infobox.AppendText("Computer Uptime: \t" + Info.Uptime + "\r\n");

            this.infobox.AppendText("\r\nUSER INFORMATION\r\n");
            this.infobox.AppendText("User Name: \t" + Info.Username + "\r\n");
            this.infobox.AppendText("Domain Name: \t" + Info.DomainName + "\r\n");
            //this.infobox.AppendText(".NET Version:: \t" + Info.DotNetVersion + "\r\n");
                        
            this.infobox.AppendText("\r\nCPU Information\r\n");
            this.infobox.AppendText("CPU Speed: \t" + Info.CpuSpeed + " GHz\r\n");        
            this.infobox.AppendText("CPU Name: \t" + Info.CpuName + "\r\n");
            this.infobox.AppendText("CPU Manufacturer: \t" + Info.CpuManufacturer + "\r\n");
            this.infobox.AppendText("Data Width: \t" + Info.DataWidth + " bit\r\n");

            this.infobox.AppendText("\r\nMEMORY INFORMATION\r\n");
            this.infobox.AppendText("System Memory: \t" + Info.TotalPhysicalMemory + "\r\n");

            this.infobox.AppendText("\r\nSTORAGE INFORMATION\r\n");
            this.infobox.AppendText("Disk Size: \t" + Info.TotalDiskSize + "\r\n");

            this.infobox.AppendText("\r\nMISC INFORMATION\r\n");
            this.infobox.AppendText("Service Tag: \t" + Info.ServiceTag + "\r\n");

            this.infobox.AppendText("\r\nSECURITY INFORMATION\r\n");
            this.infobox.AppendText("Antivirus Package: \t" + Info.AntivirusName + "\r\n");
            this.infobox.AppendText("Antivirus Status: \t" + Info.AntivirusState + "\r\n");
       
            this.infobox.AppendText("\r\nNETWORKING INFORMATION\r\n");
            this.infobox.AppendText(Info.NetworkInformation);
 
 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.infobox.Text);
        }
                
    }
}
