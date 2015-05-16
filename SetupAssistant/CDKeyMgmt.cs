using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SetupAssistant
{
    public partial class CDKeyMgmt : Form
    {
        

        public CDKeyMgmt()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;

            

            using (RegistryKey myKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Office", true))
            {
                string[] subkeys = myKey.GetSubKeyNames();

                foreach (string s in subkeys)
                {
                    switch (s)
                    {
                        case "11.0":
                            office2003btn.Enabled = true;
                            break;

                        case "12.0":
                            office2007btn.Enabled = true;
                            break;
                    }
                }
            }

            RegistryKey x64Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Office", true);

            if(x64Key != null)
            {
                string[] subkeys = x64Key.GetSubKeyNames();

                foreach (string s in subkeys)
                {
                    switch (s)
                    {
                        case "11.0":
                            x64office2003btn.Enabled = true;
                            break;

                        case "12.0":
                            x64office2007btn.Enabled = true;
                            break;
                    }
                }
            }

        }

        private void office2003btn_Click(object sender, EventArgs e)
        {
            hackRegistry("11.0", false);
        }

        private void office2007btn_Click(object sender, EventArgs e)
        {
            hackRegistry("12.0", false);
        }

        private void x64office2003btn_Click(object sender, EventArgs e)
        {
            hackRegistry("11.0", true);
        }

        private void x64office2007btn_Click(object sender, EventArgs e)
        {
            hackRegistry("12.0", true);
        }


       

        private void hackRegistry(string officeversion, bool x64os)
        {
            string key; 

            if(x64os)
                key = @"SOFTWARE\Wow6432Node\Microsoft\Office\" + officeversion + @"\Registration\";
            else
                key = @"Software\Microsoft\Office\" + officeversion + @"\Registration\";
                

            using (new CenterWinDialog(this))
            {
                try
                {
                    using (RegistryKey myKey = Registry.LocalMachine.OpenSubKey(key, true))
                    {
                        string[] subkeys = myKey.GetSubKeyNames();

                        //MessageBox.Show(subkeys[0]);

                        foreach (string s in subkeys)
                        {
                            using (RegistryKey m = Registry.LocalMachine.OpenSubKey(key + s, true))
                            {
                                string[] names = m.GetValueNames();

                                if (names.Contains("ProductID") && names.Contains("DigitalProductID"))
                                {
                                    string productName = (string)m.GetValue("ProductName");
                                    string message = "Would you like to delete CD keys for the following product?\r\n\r\n\t" + productName;
                                    message += "\r\n\t" + (string)m.GetValue("ProductID");


                                    if (MessageBox.Show(message, "Confirm Registry Key Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        m.DeleteValue("ProductID");
                                        m.DeleteValue("DigitalProductID");
                                    }

                                }

                            }
                        }

                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("There were no registry keys found for this product.", "Registry Key Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }


        }

       
        

        
    }
}
