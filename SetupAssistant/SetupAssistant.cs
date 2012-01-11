using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

using System.ServiceProcess;
using wInformation;

using Microsoft.Win32;
using System.Runtime.InteropServices;

using System.Net;
using System.Reflection;

using System.Xml;

using Ionic.Zip;


namespace SetupAssistant
{
        
    public partial class SetupAssistant : Form
    {
        [DllImport("mpr.dll")]
        private static extern int WNetDisconnectDialog(IntPtr phWnd, RESOURCETYPE piType);
        public enum RESOURCETYPE : int { ANY = 0, DISK = 1, PRINT = 2}

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp);
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_CONTEXTHELP = 0xf180;


        private string APPDIR = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Setup Assistant\\";
        private string ERRORLOG = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Setup Assistant\\log.txt";

        private string CACHE = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Setup Assistant\\Cache\\";

        private string HOSTS_FILE = System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\drivers\\etc\\hosts";
        private string HOTSTS_BACKUP = System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\drivers\\etc\\hosts" + ".sa.backup";

        private string VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private string SHORT_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 5);

        private string NORMAL_DOT   = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Templates\\Normal.dot";
        private string NORMAL_DOTM = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Templates\\Normal.dotm";

        private string NORMAL_DOT_BACKUP = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Templates\\Normal.dot" + ".sa.backup";
        private string NORMAL_DOTM_BACKUP = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Templates\\Normal.dotm" + ".sa.backup";

        private string WEB_ERROR = @"Unable to download the requested application.

The website seems to be down or blocked.
Please try again later. If the problem continues
please file a bug report at http://sa.maciak.net";
                
        private System.Windows.Forms.Label helpLabel;

        public SetupAssistant()
        {
            InitializeComponent();

            label3.Text = "version " + SHORT_VERSION;
            //label6.Text = "Version: " + VERSION;

            this.CacheAge.Value = Properties.Settings.Default.CacheAgeInDays;

            char[] alphabet = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char) i).ToArray();
            string[] drives = System.IO.Directory.GetLogicalDrives();

            foreach (char d in alphabet)
            {
                string cur = d.ToString().ToUpper() + ":";
                bool cur_drive_not_in_use = true;

                foreach (string dr in drives)
                {
                    //MessageBox.Show(dr.ToUpper());
                    if (dr.ToUpper().Equals(cur + "\\"))
                    {
                        cur_drive_not_in_use = false;
                        break;
                    }
                }
                        
                if(cur_drive_not_in_use)                       
                    comboBox1.Items.Add(cur);
            }
            
            comboBox1.SelectedIndex = 0;
          
            // check if CACHE exists
            if (!Directory.Exists(CACHE))
                Directory.CreateDirectory(CACHE);

            CalculateCacheSize();

            bool hosts_readonly = new FileInfo(HOSTS_FILE).IsReadOnly;

            if (hosts_readonly)
                button104.Text = "Set to Read/Write";
            else
                button104.Text = "Set to Read Only";

            // disable buttons that won't work on this system

            // 64 bit program files
            string dir = System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)");
            if (Directory.Exists(dir)) button107.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("HOMEDRIVE") + "\\Users\\";
            if (Directory.Exists(dir)) button72.Enabled = true;

            // Local settings
            dir = System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Local Settings\\";            
            try { Directory.GetDirectories(dir); button3.Enabled = true; }
            catch (UnauthorizedAccessException) { }

            dir = System.Environment.GetEnvironmentVariable("HOMEDRIVE") + "\\Documents and Settings\\";
            try { Directory.GetDirectories(dir); button23.Enabled = true; }
            catch (UnauthorizedAccessException) { }

            dir = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Mozilla\\Firefox\\";
            if (Directory.Exists(dir)) button22.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\boot.ini";
            if (File.Exists(dir)) button80.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Local Settings\\Application Data\\Microsoft\\Outlook\\";
            if (Directory.Exists(dir)) button4.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Outlook\\";
            if (Directory.Exists(dir)) button5.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Excel";
            if (Directory.Exists(dir)) button87.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Word";
            if (Directory.Exists(dir)) button88.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft";
            if (Directory.Exists(dir)) button131.Enabled = true;
                        
            if (File.Exists(NORMAL_DOT) || File.Exists(NORMAL_DOTM))
            {
                button132.Enabled = true;
                button133.Enabled = true;
            }

            if (File.Exists(NORMAL_DOT_BACKUP) || File.Exists(NORMAL_DOTM_BACKUP))
            {
                button134.Enabled = true;
                button135.Enabled = true;
            }


            dir = System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + "\\Microsoft Office";
            if (Directory.Exists(dir)) button20.Enabled = true;
            if (Directory.Exists(dir)) button21.Enabled = true;
            if (Directory.Exists(dir)) button8.Enabled = true;

            dir = System.Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\Microsoft Office";
            if (Directory.Exists(dir))
            {
                button20.Enabled = true; // office program folder
                button21.Enabled = true;
                button8.Enabled = true;
                button173.Enabled = true; // office key management
            }

            dir = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Local Settings\Application Data\Identities\";
            if (Directory.Exists(dir)) button160.Enabled = true;


            dir = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\OFFICE10\";
            if (Directory.Exists(dir)) button167.Enabled = true;

            dir = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\OFFICE11\";
            if (Directory.Exists(dir)) button165.Enabled = true;

            dir = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\OFFICE12\";
            if (Directory.Exists(dir)) button166.Enabled = true;

            dir = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\OFFICE10\";
            if (Directory.Exists(dir)) button167.Enabled = true;

            dir = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\OFFICE11\";
            if (Directory.Exists(dir)) button165.Enabled = true;

            dir = Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\OFFICE12\";
            if (Directory.Exists(dir)) button166.Enabled = true;

            dir = Environment.GetEnvironmentVariable("WINDIR") + @"\Minidump\";
            if (Directory.Exists(dir)) button63.Enabled = true;

        }

        private void button_HelpRequested(object sender, System.Windows.Forms.HelpEventArgs hlpevent)
        {            
            Control requestingControl = (Control)sender;
            string helptext = (string)requestingControl.Tag;

            using (new CenterWinDialog(this))
            {
                if (helptext != null || helptext != "")
                    MessageBox.Show(this, helptext, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            hlpevent.Handled = true;
        }



        

        private void runFromCacheOrDownloadZipfile(string filename, string url, string zipfile, bool commandline, string command_args)
        {
            string path = CACHE + filename;
            string zippath = CACHE + zipfile;

            DateTime lastwrite = File.GetLastWriteTime(path);
            int date_comparison_result = DateTime.Compare(lastwrite, getAcceptableCacheAge());

            // if the file exist just do the usual bullshit - run from chache
            if (File.Exists(path) && date_comparison_result > 0)
                if (commandline)
                {
                    string args = "/k \"" + path + "\"" + command_args;
                    this.runElevated("cmd", args);
                }
                else
                    Process.Start(path);
            else
            {
                // else we download the zip file, unzip it and then do the usual

                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    Uri uri = new Uri(url);
                    downloadFileToCache(uri, zippath);

                    CalculateCacheSize();

                    using (ZipFile z = ZipFile.Read(zippath))
                    {
                        foreach (ZipEntry e in z)
                        {
                            //MessageBox.Show(e.FileName);
                            if (e.FileName == filename)
                                e.Extract(CACHE, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    if (File.Exists(path))
                        if(commandline)
                            this.runElevated("cmd", "/k \"" + path + "\"");
                        else
                            Process.Start(path);
                    else
                        using (new CenterWinDialog(this)) { MessageBox.Show(this, "There was a problem downloading the file.", "File Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }

                }
                catch (WebException we) { using (new CenterWinDialog(this)) { MessageBox.Show(this, WEB_ERROR, "Web Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }  }
                catch (Exception ee) { using (new CenterWinDialog(this)) { MessageBox.Show(this, ee.Message + "\r\n\r\n" + ee.ToString(), "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }  }
                finally { this.Cursor = Cursors.Default; }

            }
            
        }

        private void runFromCacheOrDownloadZipfile(string filename, string url, string zipfile, bool commandline)
        {
            runFromCacheOrDownloadZipfile(filename, url, zipfile, commandline, "");
        }

        private void runFromCacheOrDownloadZipfile(string filename, string url, string zipfile)
        {
            runFromCacheOrDownloadZipfile(filename, url, zipfile, false);
        }

        private DateTime getAcceptableCacheAge()
        {
            DateTime acceptable_cache_age;

            if (Properties.Settings.Default.CacheAgeInDays == 0) // caching off (always redownload)
                acceptable_cache_age = DateTime.MaxValue; // set to max value so that everything is earlier
            else if (Properties.Settings.Default.CacheAgeInDays == -1) // expire off - always use cache if available
                acceptable_cache_age = DateTime.MinValue; // set to min so everything is later
            else
                acceptable_cache_age = DateTime.Now - TimeSpan.FromDays((double)Properties.Settings.Default.CacheAgeInDays);

            return acceptable_cache_age;
        }

        private void runFromCacheOrDownload(string filename, string url)
        {
            runFromCacheOrDownload(filename, url, null);
        }
        private void runFromCacheOrDownload(string filename, string url, string referrer)
        {
            string path = CACHE + filename;

            DateTime acceptable_cache_age;

            acceptable_cache_age = getAcceptableCacheAge();

            DateTime lastwrite = File.GetLastWriteTime(path);

            // negative if file date is earlier than acceptable cache age
            int date_comparison_result = DateTime.Compare(lastwrite, acceptable_cache_age);

            if (File.Exists(path) && date_comparison_result > 0)
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception exc)
                {
                    using (new CenterWinDialog(this))
                    {
                        if (MessageBox.Show("Error running cached application. Would you like to clear application cache?", "Cached Application Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                            ClearCache();
                    }
                }
            }
            else
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    Uri uri = new Uri(url);
                    downloadFileToCache(uri, path, referrer);
                    
                    CalculateCacheSize();

                    Process.Start(path);
                }
                catch (WebException we) { using (new CenterWinDialog(this)) { MessageBox.Show(this, WEB_ERROR, "Web Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }  }
                finally { this.Cursor = Cursors.Default; }
            }
        }

        private void downloadFileToCache(Uri url, string path)
        {
            downloadFileToCache(url, path, null);
        }

        private void downloadFileToCache(Uri url, string path, string referrer)
        {
                WebClient webClient = new WebClient();
                //webClient.Credentials = CredentialCache.DefaultCredentials;
                webClient.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "anything");
                if(referrer != null) webClient.Headers.Add("Referer", referrer);

                webClient.DownloadFile(url, path);
        }

        protected void CalculateCacheSize()
        {
            double folderSize = 0.0d;

            try
            {
                foreach (string file in Directory.GetFiles(CACHE))
                {
                    if (File.Exists(file))
                    {
                        FileInfo finfo = new FileInfo(file);
                        folderSize += finfo.Length;
                     }
                 }

                        
            }
            catch (Exception e) { using (new CenterWinDialog(this)) { MessageBox.Show(this, e.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }  }
                        
            double size = Math.Round((folderSize / 1024d) / 1024d, 2);

            this.label5.Text = size + " MB";
        }





        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("APPDATA"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
           Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Local Settings\\");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try 
            {
                Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Local Settings\\Application Data\\Microsoft\\Outlook\\");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Office probably not installed", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Outlook\\");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Office probably not installed", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Process.Start("ncpa.cpl");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process.Start("inetcpl.cpl");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string dir86 = System.Environment.GetEnvironmentVariable("PROGRAMFILES(x86)");
            string dir32 = System.Environment.GetEnvironmentVariable("PROGRAMFILES");

            string folder = Directory.Exists(dir86) ? dir86 + "\\Microsoft Office\\" : dir32 + "\\Microsoft Office\\";
            
            try
            {
                if (File.Exists(folder + "\\Office11\\mlcfg32.cpl"))
                    Process.Start("rundll32.exe", "shell32.dll,Control_RunDLL \"" + folder + "\\Office11\\mlcfg32.cpl\"");
                else if (File.Exists(folder + "\\Office12\\mlcfg32.cpl"))
                    Process.Start("rundll32.exe", "shell32.dll,Control_RunDLL \"" + folder + "\\Office12\\mlcfg32.cpl\"");
                else
                    Process.Start("control", "mlcfg32.cpl");

            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Office probably not installed", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }

            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Process.Start("::{2227A280-3AEA-1069-A2DE-08002B30309D}");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
               Process.Start("nusrmgr.cpl");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Not supported by your OS", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void spooler(Boolean start)
        {
            startThisService("Spooler", start);
        }

        private void startThisService(string service_name, Boolean true_to_start_false_to_stop)
        {
            try
            {
                ServiceController scController = new ServiceController();
                scController.ServiceName = service_name;
                scController.MachineName = System.Windows.Forms.SystemInformation.ComputerName;
                
                if(true_to_start_false_to_stop)
                    scController.Start();
                else
                    scController.Stop();

                //System.Threading.Thread.Sleep(50);

                if (true_to_start_false_to_stop)
                    scController.WaitForStatus(ServiceControllerStatus.Running);
                else
                    scController.WaitForStatus(ServiceControllerStatus.Stopped);

                using (new CenterWinDialog(this))
                {
                    string sStatus = scController.Status.ToString();
                    
                    using (new CenterWinDialog(this))
                        MessageBox.Show(service_name + " is now " + sStatus);
                }

            }
            catch (Exception Ex)
            {
                using (new CenterWinDialog(this))
                    MessageBox.Show(Ex.Message);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            this.spooler(false);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            this.spooler(true);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\spool\\PRINTERS\\");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\drivers\\etc\\");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("TEMP"));
        }

        private void button16_Click(object sender, EventArgs e)
        {
            try { Process.Start("devmgmt.msc"); }
            catch { }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("services.msc");
            }
            catch { }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try { Process.Start("eventvwr.msc"); }
            catch { }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\Fonts\\");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\Microsoft Office\\");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Office probably not installed", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            string dir86 = System.Environment.GetEnvironmentVariable("PROGRAMFILES(x86)");
            string dir32 = System.Environment.GetEnvironmentVariable("PROGRAMFILES");

            string folder = Directory.Exists(dir86) ? dir86 + "\\Microsoft Office\\" : dir32 + "\\Microsoft Office\\";

            try
            {
                if (File.Exists(folder + "\\Office11\\scanpst.exe"))
                    Process.Start(folder + "\\Office11\\scanpst.exe");
                else if (File.Exists(folder + "\\Office12\\scanpst.exe"))
                    Process.Start(folder + "\\Office12\\scanpst.exe");
                else
                    Process.Start("scanpst");
            }
            catch (Exception ex)
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, ex.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Mozilla\\Firefox\\");
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("HOMEDRIVE") + "\\Documents and Settings\\");
            }
            catch (Exception x) { using (new CenterWinDialog(this)) { MessageBox.Show(this, x.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("ALLUSERSPROFILE"));
        }

        private void button25_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\Temp\\");
        }

        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                wSystemInformation info = new wSystemInformation();

                Form I = new Info(info);
                I.ShowDialog(this);
            }
            catch (Exception ex)
            { using (new CenterWinDialog(this)) { MessageBox.Show(this, ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error); } }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            try { Process.Start("diskmgmt.msc"); }
            catch { }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("dfrg.msc");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Not available in your OS", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button29_Click(object sender, EventArgs e)
        {
            try { Process.Start("cleanmgr"); }
            catch { }
        }

        private void button30_Click(object sender, EventArgs e)
        {
            try { Process.Start("nusrmgr.cpl"); }
            catch { }

            try { Process.Start("control", "userpasswords"); }
            catch { }
        }

        private void button31_Click(object sender, EventArgs e)
        {
            Process.Start("control", "folders");
        }

        private void button32_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("control", "sysdm.cpl");
            }
            catch { }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            try { Process.Start("regedit"); }
            catch { }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            try { Process.Start("msconfig"); }
            catch { }
        }

        private void button35_Click(object sender, EventArgs e)
        {
            try { Process.Start("msinfo32"); }
            catch { }
        }

        private void button36_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("taskmgr");
        }

        private void button37_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd");
        }

        private void button38_Click(object sender, EventArgs e)
        {
            try { System.Diagnostics.Process.Start("dfrgui"); }
            catch { }

            try
            {
                Process.Start("dfrg.msc");
            }
            catch
            {}

        }

        private void button39_Click(object sender, EventArgs e)
        {
            Form ping = new PingBox();
            ping.ShowDialog(this);
        }

        private void button40_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("osk");
        }

        private void runElevated(string path, string args)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            
            // no longer needed as of 0.2 because the whole application runs elevated now            
            //processInfo.Verb = "runas";
            
            processInfo.FileName = path;
            processInfo.Arguments = args;

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception x) { using (new CenterWinDialog(this)) { MessageBox.Show(this, x.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
            
            
        }


        private void button43_Click(object sender, EventArgs e)
        {
            this.runElevated("cmd", "/k sfc /scannow");
        }

        private void button44_Click(object sender, EventArgs e)
        {
            this.runElevated("cmd", "/k chkdsk " + System.Environment.GetEnvironmentVariable("SYSTEMDRIVE") + " /r");
        }

        private void button41_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Start Menu\\Programs\\Startup\\");
        }

        private void button42_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("ALLUSERSPROFILE") + "\\Start Menu\\Programs\\Startup\\");
        }

        private void button45_Click(object sender, EventArgs e)
        {
            Process.Start("notepad");

            //MessageBox.Show("X: " + this.Location.X + " Y: " + this.Location.Y);
        }

        private void button46_Click(object sender, EventArgs e)
        {
            this.runAndCapture("cmd", "/c ipconfig /all", "Network Configuration");
        }

        private void button47_Click(object sender, EventArgs e)
        {
            this.runAndCapture("cmd", "/c ipconfig /displaydns", "DNS Cache");
            
        }

        private void runAndCapture(string command, string args, string label)
        {
                                                         
            try
            {
                string output = ProcessRunner.runAndCapture(command, args);

                InformationBox f = new InformationBox(output, label);
                f.ShowDialog(this);
                //f.display(output, label);
            }
            catch (Exception e)
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, e.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }         
        }

        private void displayFile(string file, string label)
        {
            try
            {
                
                System.IO.StreamReader reader = File.OpenText(file);
                string output = reader.ReadToEnd();
                reader.Close();


                InformationBox f = new InformationBox(output, label);
                //f.display(output, label);
                f.ShowDialog(this);
            }
            catch (Exception e)
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, e.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            } 
        }

        
        private void button48_Click(object sender, EventArgs e)
        {
            this.runElevated("cmd", "/c ipconfig /flushdns");
        }

        private void button49_Click(object sender, EventArgs e)
        {
            this.runElevated("cmd", "/c ipconfig /release");
        }

        private void button50_Click(object sender, EventArgs e)
        {
            this.runElevated("cmd", "/c ipconfig /renew");
        }

        private void runAndRedirect(string command, string args, string label)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = command;
            processInfo.Arguments = args;
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.CreateNoWindow = true;

            try
            {
                Process p = Process.Start(processInfo);
                System.IO.StreamReader reader = p.StandardOutput;
                                
                string output = "";
                string text = "";

                this.Cursor = Cursors.WaitCursor;

                while ((output = reader.ReadLine()) != null)
                {
                    if (output != "")
                    {
                        text += output + "\r\n";
                        
                    }
                }

                reader.Close();

                this.Cursor = Cursors.Default;

                InformationBox f = new InformationBox(text, label);
                f.ShowDialog(this);
            }
            catch (Exception e)
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, e.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }

        }

        private void button51_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                if (MessageBox.Show("This is really slow, are you sure you want to wait?", "Sloooow!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    this.runAndRedirect("cmd", "/c netstat -b -a", "Netstat Results");
            }

        }

        private void button52_Click(object sender, EventArgs e)
        {
            this.runAndCapture("cmd", "/c netstat -s", "Network Statistics");
        }

        private void button53_Click(object sender, EventArgs e)
        {
            this.runElevated("cmd", "/k");
        }

        private void button54_Click(object sender, EventArgs e)
        {
            Form nt = new NetTester();
            nt.ShowDialog(this);
        }

        private void button55_Click(object sender, EventArgs e)
        {
            string url = "http://live.sysinternals.com/procexp.exe";
            string file = "procexp.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button56_Click(object sender, EventArgs e)
        {
            string url = "http://live.sysinternals.com/autoruns.exe";
            string file = "autoruns.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button57_Click(object sender, EventArgs e)
        {
            string url = "http://live.sysinternals.com/procmon.exe";
            string file ="procmon.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button58_Click(object sender, EventArgs e)
        {
            string url = "http://live.sysinternals.com/TcpView.exe";
            string file = "TcpView.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button61_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES"));
        }

        private void button59_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR"));
        }

        private void button60_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\");
        }

        private void button53_Click_1(object sender, EventArgs e)
        {
            //Process.Start("iexplore", " http://www.nirsoft.net/panel/bluescreenview.exe");

            string url = "http://www.nirsoft.net/panel/bluescreenview.exe";           
            string filename = "bluescreenview.exe";
          

            runFromCacheOrDownload(filename, url);
        }

        private void button62_Click(object sender, EventArgs e)
        {
            string url = "http://www.nirsoft.net/panel/nk2view.exe";
            string file = "nk2view.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button64_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Local Settings\\History\\");
        }

        private void button65_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Recent\\");
        }

        private void button66_Click(object sender, EventArgs e)
        {
            Process.Start("control", "appwiz.cpl");

        }

        private void button67_Click(object sender, EventArgs e)
        {
            string url = "http://www.nirsoft.net/panel/officeins.exe";
            string file = "officeins.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button68_Click(object sender, EventArgs e)
        {
            string url = "http://www.nirsoft.net/panel/produkey.exe";
            string file = "produkey.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button69_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("msra");
            }
            catch (Exception x) { using (new CenterWinDialog(this)) { MessageBox.Show(this, x.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
            
        }

        private void button70_Click(object sender, EventArgs e)
        {
            Process.Start("mstsc");
        }

        private void button71_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore", " http://join.me");
        }

        private void button71_Click_1(object sender, EventArgs e)
        {
            Process.Start("prefetch");
        }

        private void button63_Click(object sender, EventArgs e)
        {
            Process.Start("minidump");
        }

        private void button72_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("HOMEDRIVE") + "\\Users\\");
            }
            catch (Exception x) { using (new CenterWinDialog(this)) { MessageBox.Show(x.Message); } }
        }

        private void button73_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("When prompted, please choose Save rather than Run. HijackThis should not be run from a temporary directory.");

            string url = "http://www.trendmicro.com/ftp/products/hijackthis/HijackThis.exe";
            string file = "HijackThis.exe";

            runFromCacheOrDownload(file, url);
            
        }

        private void button74_Click(object sender, EventArgs e)
        {
            string url = "http://www.atribune.org/ccount/click.php?id=1";
            string file = "ATF-Cleaner.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button75_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Local Settings\\Temporary Internet Files\\");
        }

        private void button76_Click(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/sUBs/ComboFix.exe";
            string file = "ComboFix.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button77_Click(object sender, EventArgs e)
        {
            this.displayFile(HOSTS_FILE, "HOSTS File");
        }

        private void button78_Click(object sender, EventArgs e)
        {
            string url = "http://www.freedrweb.com/cureit/";
            //string file = "drweb-cureit.exe";

            Process.Start(url);
            //runFromCacheOrDownload(file, url);
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            Process.Start("compmgmt.msc");
        }

        private void button79_Click(object sender, EventArgs e)
        {
            Process.Start("tasks");
        }

        private void button80_Click(object sender, EventArgs e)
        {
            Process.Start("notepad", System.Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\boot.ini");
        }

        private void button81_Click(object sender, EventArgs e)
        {
            Process.Start("notepad", System.Environment.GetEnvironmentVariable("WINDIR") + "\\win.ini");
        }

        private void button82_Click(object sender, EventArgs e)
        {
            Process.Start("notepad", System.Environment.GetEnvironmentVariable("WINDIR") + "\\system.ini");
        }

        private void button28_Click_1(object sender, EventArgs e)
        {
            Process.Start("http://www.cpuid.com/softwares/cpu-z.html");
        }

        private void button84_Click(object sender, EventArgs e)
        {
            string software = "";
            
            software += "Installed Software \r\n";
            software += "================== \r\n\r\n";

            string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
            {
                foreach (string name in rk.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subkey = rk.OpenSubKey(name))
                        {
                            // if subkey value is null, skip
                            if (!(subkey.GetValue("DisplayName") == null))
                            {
                                if (subkey.GetValue("InstallLocation") == null || subkey.GetValue("InstallLocation").Equals(String.Empty))
                                    software += " -- " + subkey.GetValue("DisplayName") + " \r\n\r\n";
                                else
                                    software += " -- " + subkey.GetValue("DisplayName") + "\r\n\t +-- " + subkey.GetValue("InstallLocation") + "\r\n\r\n";
                            }
                        }
                    }
                    catch{}

                }

                InformationBox f = new InformationBox(software, "Installed Software");
                //f.display(software, "Installed Software");
                f.ShowDialog(this);
            }
        }

        private void button85_Click(object sender, EventArgs e)
        {
            Process[] processlist = Process.GetProcesses();

            string processes = "";

            processes += "Running Processes by Id \r\n";
            processes += "======================= \r\n\r\n";

            IEnumerable<Process> q =  processlist.OrderBy(proc => proc.Id);

            foreach(Process theprocess in q)
            {
                processes += "" + theprocess.Id + " \t-->\t" + theprocess.ProcessName  + " \r\n";
            }

            InformationBox f = new InformationBox(processes, "Running Processes");
            //f.display(processes, "Running Processes");
            f.ShowDialog(this);
        }

        private void button83_Click(object sender, EventArgs e)
        {
            Process.Start("regsvr32", this.dllinputbox.Text);
            dllinputbox.Clear();
            dllinputbox.Focus();
            dllinputbox.Select();
        }

        private void button86_Click(object sender, EventArgs e)
        {
            Process.Start("regsvr32", "/u " + this.dllinputbox.Text);
            dllinputbox.Clear();
            dllinputbox.Focus();
            dllinputbox.Select();
        }

        private void button20_Click_1(object sender, EventArgs e)
        {
            
            if(Directory.Exists(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)")+ "\\Microsoft Office"))
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + "\\Microsoft Office");
            else
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES") + "\\Microsoft Office");
        }

        private void button87_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Excel");
            }
            catch { }
        }

        private void button88_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft\\Word");
            }
            catch { }
        }

        private void button89_Click(object sender, EventArgs e)
        {
            Process.Start("http://crossloop.com");
        }

        private void button90_Click(object sender, EventArgs e)
        {
            Process.Start("http://join.me");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://malwarebytes.org");

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://superantispyware.com");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.x-raypc.com");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.hijackthis.de/en");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/details.aspx?FamilyId=941b3470-3ae9-4aee-8f43-c6bb74cd1466");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.biblet.freeserve.co.uk/");
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/en/details.aspx?familyid=f5539a90-dc41-4792-8ef8-f4de62ff1e81&displaylang=en");
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/en/details.aspx?FamilyId=262D25E3-F589-4842-8157-034D1E7CF3A3&displaylang=en");
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/en/details.aspx?FamilyId=333325fd-ae52-4e35-b531-508d977d32a6&displaylang=en");
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/en/details.aspx?FamilyID=9cfb2d51-5ff4-4491-b0e5-b386f32c0992&displaylang=en");
        }

        private void button91_Click(object sender, EventArgs e)
        {
            Process.Start("inetcpl.cpl");
        }

        private void button92_Click(object sender, EventArgs e)
        {
            Process.Start("notepad", HOSTS_FILE);
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.piriform.com/ccleaner");
        }

        private void linkLabel12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/en/details.aspx?familyid=1B286E6D-8912-4E18-B570-42470E2F3582&displaylang=en");
        }

        private void linkLabel13_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://sa.maciak.net");
        }

        private void linkLabel14_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://sites.maciak.net/setup-assistant");
        }

        private void button95_Click(object sender, EventArgs e)
        {
            runAndCapture("net", "use", "Connected Network Drives");
        }

        private void button93_Click(object sender, EventArgs e)
        {
            Process.Start("net", "use " + this.comboBox1.SelectedItem + " " + this.textBox2.Text);
        }

        private void button94_Click(object sender, EventArgs e)
        {
            //try { Process.Start("rundll32.exe", " shell32.dll,SHHelpShortcuts_RunDLL Disconnect"); }
            //catch (Exception ex) { MessageBox.Show(ex.Message); }

            try
            {
                int i = WNetDisconnectDialog(IntPtr.Zero, RESOURCETYPE.DISK);
                if (i > 0) { throw new System.ComponentModel.Win32Exception(i); }
            }
            catch (Exception ex) { using (new CenterWinDialog(this)) { MessageBox.Show(this, ex.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
        }

        private void button96_Click(object sender, EventArgs e)
        {
            try { Process.Start("rundll32.exe", " shell32.dll,SHHelpShortcuts_RunDLL Connect"); }
            catch (Exception ex) { using (new CenterWinDialog(this)) { MessageBox.Show(this, ex.Message, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
        }

        private void button97_Click(object sender, EventArgs e)
        {
            Process.Start(CACHE);
        }

        private void button98_Click(object sender, EventArgs e)
        {
            this.ClearCache();
        }

        private void ClearCache()
        {
            Directory.Delete(CACHE, true);
            Directory.CreateDirectory(CACHE);
            CalculateCacheSize();
        }

        private void backupFile(string filename, string backupname)
        {
            if(File.Exists(filename))
                File.Copy(filename, backupname, true);
        }

        private void hostsBackup()
        {
            if (File.Exists(HOSTS_FILE))
                backupFile(HOSTS_FILE, HOTSTS_BACKUP);
        }

        private void deleteHostsBackup()
        {
            if (File.Exists(HOTSTS_BACKUP))
                File.Delete(HOTSTS_BACKUP);           
        }
       
        private void button99_Click(object sender, EventArgs e)
        {
            if(File.Exists(HOSTS_FILE))
            {
                hostsBackup();
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(this, "HOSTS backed up to: " + HOTSTS_BACKUP, "HOSTS Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "No HOSTS file present... Try recreating the file.", "HOSTS Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }

        }

        private void button100_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\drivers\\etc\\");
        }

        private void button101_Click(object sender, EventArgs e)
        {
          
            if(File.Exists(HOTSTS_BACKUP))
            {
                deleteHostsBackup();
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(this, "Deleted " + HOTSTS_BACKUP, "HOSTS Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "No HOSTS backup file present... Nothing was deleted", "Hosts Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void createHostsFile()
        {
            string hostsText = @"# Copyright (c) 1993-1999 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
# This file contains the mappings of IP addresses to host names. Each
# entry should be kept on an individual line. The IP address should
# be placed in the first column followed by the corresponding host name.
# The IP address and the host name should be separated by at least one
# space.
#
# Additionally, comments (such as these) may be inserted on individual
# lines or following the machine name denoted by a '#' symbol.
#
# For example:
#
#      102.54.94.97     rhino.acme.com          # source server
#       38.25.63.10     x.acme.com              # x client host

127.0.0.1       localhost";



            TextWriter tw = new StreamWriter(HOSTS_FILE);
            tw.Write(hostsText);
            tw.Close();

        }

        private void button102_Click(object sender, EventArgs e)
        {
            string message = "";
            if (File.Exists(HOSTS_FILE))
            {
                hostsBackup();
                File.Delete(HOSTS_FILE);
                message = "HOSTS backed up to " + HOTSTS_BACKUP + "\r\n\r\n";
            }
            
            createHostsFile();
            using (new CenterWinDialog(this))
                MessageBox.Show(this, message + "HOSTS file was recreated using default settings.", "HOSTS Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void button103_Click(object sender, EventArgs e)
        {
            if (File.Exists(HOTSTS_BACKUP))
            {
                File.Copy(HOTSTS_BACKUP, HOSTS_FILE, true);
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "HOSTS file was restored from backup", "HOSTS Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "No backup found... Could not restore", "HOSTS Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button104_Click(object sender, EventArgs e)
        {
            FileInfo f = new FileInfo(HOSTS_FILE);

            if (f.IsReadOnly)
            {
                File.SetAttributes(HOSTS_FILE, FileAttributes.Normal);
                this.button104.Text = "Set to Read Only";
            }
            else
            {
                File.SetAttributes(HOSTS_FILE, FileAttributes.ReadOnly);
                this.button104.Text = "Set to Read/Write";
            }

        }

        private void folderBarricade(bool turn_on)
        {
            string registry_key = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\WebView\\BarricadedFolders";
            int value = turn_on ? 1 : 0;
            
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(registry_key, true);

            registry.SetValue("shell:ProgramFiles", value, RegistryValueKind.DWord);
            registry.SetValue("shell:System", value, RegistryValueKind.DWord);
            registry.SetValue("shell:SystemDriveRootFolder", value, RegistryValueKind.DWord);
            registry.SetValue("shell:Windows", value, RegistryValueKind.DWord);
        }

        private void button105_Click(object sender, EventArgs e)
        {
            folderBarricade(true);
        }

        private void button106_Click(object sender, EventArgs e)
        {
            folderBarricade(false);
        }

        private void button107_Click(object sender, EventArgs e)
        {
            string dir = System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)");

            if (Directory.Exists(dir))
                Process.Start(dir);
        }

        private void linkLabel15_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/downloads/en/details.aspx?FamilyID=0856eacb-4362-4b0d-8edd-aab15c5e04f5&displaylang=en");
            
        }


        private void linkLabel16_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.publicshareware.com/public-share-outlook-utilities.php");
        }

        private void logError(Exception e)
        {
            TextWriter tw = new StreamWriter(ERRORLOG, true);
            
            tw.Write(DateTime.Now.ToString() + "\n");
            tw.Write(e.ToString() + "\n");

            tw.Close();
        }

        private void button115_Click(object sender, EventArgs e)
        {
            string url = "http://www.trendmicro.com/ftp/products/hijackthis/HijackThis.exe";
            string file = "HijackThis.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button113_Click(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/sUBs/ComboFix.exe";

            string file = "ComboFix.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button114_Click(object sender, EventArgs e)
        {
            string url = "http://www.atribune.org/ccount/click.php?id=1";
            string file = "ATF-Cleaner.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button112_Click(object sender, EventArgs e)
        {
            string url = "http://www.freedrweb.com/cureit/";
            //string file = "drweb-cureit.exe";

            Process.Start(url);
            //runFromCacheOrDownload(file, url);
        }

        private void button73_Click_1(object sender, EventArgs e)
        {
            string url = "http://support.kaspersky.com/downloads/utils/tdsskiller.exe";
            string file = "tdsskiller.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button74_Click_1(object sender, EventArgs e)
        {
            string url = "http://secunia.com/PSISetup.exe";
            string file = "PSISetup.exe";

            runFromCacheOrDownload(file, url);
        }
       
        private void button76_Click_1(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/jpshortstuff/Defogger.exe";
            string file = "Defogger.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button78_Click_1(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/sUBs/dds.scr";
            string file = "dds.scr";

            runFromCacheOrDownload(file, url);
        
        }

        private void linkLabel17_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://www.freedrweb.com/cureit/";

            Process.Start(url);
        }

        private void linkLabel18_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://malwarebytes.org/";

            Process.Start(url);
        }

        private void linkLabel19_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://superantispyware.com/";

            Process.Start(url);
        }

        private void linkLabel20_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://secunia.com/vulnerability_scanning/personal/";
            Process.Start(url);
        }

        private void button74_Click_2(object sender, EventArgs e)
        {
            string url = "http://live.sysinternals.com/pagedfrg.exe";
            string file = "pagedfrg.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button112_Click_1(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/grinler/rkill.exe";
            string file = "rkill.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button116_Click(object sender, EventArgs e)
        {
            string url = "http://www2.gmer.net/download.php";
            string file = "gmer.exe";

            runFromCacheOrDownload(file, url);
        }

        private void linkLabel21_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://www.gmer.net/";
            Process.Start(url);
            
        }

        private void button117_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\spool\\PRINTERS\\");
        }

        private void button118_Click(object sender, EventArgs e)
        {
            string url = "http://maciak.org/tools/fixmyprinter/FixMyPrinter.exe";
            string file = "FixMyPrinter.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button119_Click(object sender, EventArgs e)
        {
            AboutBox1 a = new AboutBox1();
            a.ShowDialog(this);
        }

        private void button120_Click(object sender, EventArgs e)
        {
            

            string license = "Luke's Setup Assistant is a FREEWARE tool. \r\n\r\n" + "You are allowed to use and install it on any computer, for unlimited duration of time without any usage restictions. \r\n\r\n" + "This tools provided by the author and contributors \"as is\" and any express or implied warranties, including, but not limited to, the implied warranties of merchantability and fitness for a particular purpose are disclaimed. In no event shall the author or contributors be liable for any direct, indirect, incidental, special, exemplary, or consequential damages (including, but not limited to, procurement of substitute goods or services; loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether in contract, strict liability, or tort (including negligence or otherwise) arising in any way out of the use of this software, even if advised of the possibility of such damage.\r\n\r\nSee: http://sa.maciak.net/license";
            
            InformationBox f = new InformationBox(license, "License Information");
            f.ShowDialog(this);
            //f.display(license, "License Information");
        }

        private void button121_Click(object sender, EventArgs e)
        {
            string url = "http://maciak.org/tools/closesharedfiles/CloseSharedFiles.exe";
            string file = "CloseSharedFiles.exe";

            runFromCacheOrDownload(file, url);
        }

        private void linkLabel22_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.hijackthis.de/en");
        }

        private void linkLabel23_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.piriform.com/ccleaner");
        }

        private void button122_Click(object sender, EventArgs e)
        {
            this.runAndCapture("cmd", "/c netsh winsock show catalog", "WINSOCK Catalog");
        }

        private void linkLabel24_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.x-raypc.com");
        }

        private void button123_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                if (MessageBox.Show("After this runs, you will need to reboot your computer.\r\nDo you wish to continue?",
                                    "Reboot Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    this.runElevated("cmd", "/k netsh winsock reset");
            }
        }

        private void button124_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                if (MessageBox.Show("After this runs, you may need to reboot your computer.\r\nDo you wish to continue?",
                                    "Reboot Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (!Directory.Exists(CACHE + "tcpip_resetlogs//"))
                        Directory.CreateDirectory(CACHE + "tcpip_resetlogs//");
                }

                string resetlog_file = this.CACHE + "tcpip_resetlogs//" + Path.GetRandomFileName() + ".txt";
                Process p = Process.Start("cmd", "/c netsh int ip reset \"" + resetlog_file + "\"");
                p.WaitForExit();


                using (new CenterWinDialog(this))
                {
                    if (MessageBox.Show("Would you like to view the log file now?",
                                    "Log File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        Process.Start("notepad", resetlog_file);
                }
                 
            }
        }

        private void button125_Click(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/grinler/unhide.exe";
            string file = "unhide.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button126_Click(object sender, EventArgs e)
        {
            string url = "http://www.malwarebytes.org/mbam-clean.exe";
            string file = "mbam-clean.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button127_Click(object sender, EventArgs e)
        {
            string url = "http://maciak.org/tools/zipfixforoutlook/ZipFixForOutlook.exe";
            string file = "ZipFixForOutlook.exe";

            runFromCacheOrDownload(file, url);
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://support.microsoft.com/kb/947821");
        }

        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.virustotal.com/");
        }

        private void linkLabel3_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.eset.com/us/online-scanner");
        }

        private void linkLabel4_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.bitdefender.com/scanner/online/free.html");
        }

        private void linkLabel11_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://cainternetsecurity.net/entscanner/");
        }

        private void linkLabel25_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.f-secure.com/en_EMEA-Labs/security-threats/tools/online-scanner/");
        }

        private void linkLabel26_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.kaspersky.com/kos/eng/partner/default/kavwebscan.html");
        }

        private void linkLabel27_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.pandasecurity.com/activescan/index/");
        }

        private void linkLabel29_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://support.microsoft.com/kb/971058");
        }

        private void button128_Click(object sender, EventArgs e)
        {
            string url = "http://maciak.org/tools/closesharedfiles/CloseSharedFiles.exe";
            string file = "CloseSharedFiles.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button129_Click(object sender, EventArgs e)
        {
            string url = "http://maciak.org/tools/installercheck/InstallerCheck.exe";
            string file = "InstallerCheck.exe";

            runFromCacheOrDownload(file, url);
        }



        private void checkVersion()
        {
             
            Version newVersion = null;
            string versionURL = "http://sa.maciak.net/version.txt";
            string downloadURL = "http://sa.maciak.net/download";

            HttpWebRequest hwRequest = (HttpWebRequest)WebRequest.Create(versionURL);
            hwRequest.Timeout = 15000;
            HttpWebResponse hwResponse = null;


            try
            {
                this.Cursor = Cursors.WaitCursor;

                hwResponse = (HttpWebResponse)hwRequest.GetResponse();
                Stream receiveStream = hwResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receiveStream, encode);

                string ver = readStream.ReadLine();
                newVersion = new Version(ver);

                //MessageBox.Show(ver);
            }
            catch (Exception ex)
            {
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

            // get the running version  
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            using (new CenterWinDialog(this))
            {

                if (curVersion.CompareTo(newVersion) < 0)
                {

                    string title = "New Version Available";
                    string question = "Installed Version: " + curVersion.ToString() + "\r\n" +
                                      "Latest Version: " + newVersion.ToString() + "\r\n" +
                                      "Would you like to download the latest version of Luke's Setup Assistant?";
                    using (new CenterWinDialog(this)) 
                    {
                        if (DialogResult.Yes == MessageBox.Show(this, question, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        {
                            Process.Start(downloadURL);
                        }
                    }
                }
                else
                    using (new CenterWinDialog(this)) { MessageBox.Show(this, "Luke's Setup Assistant is up to date.", "No New Version Available", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); }

            }
        }

        private void button130_Click(object sender, EventArgs e)
        {
            this.checkVersion();
        }

        private void button131_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("APPDATA") + "\\Microsoft");
            }
            catch (Exception)
            { }
        }

        private void backupNormal()
        {
            if (File.Exists(NORMAL_DOT))
            {                
                backupFile(NORMAL_DOT, NORMAL_DOT_BACKUP);
                using (new CenterWinDialog(this))
                    MessageBox.Show("Normal.dot backed up to: " + NORMAL_DOT_BACKUP);
            }

            if (File.Exists(NORMAL_DOTM))
            {
                backupFile(NORMAL_DOTM, NORMAL_DOTM_BACKUP);
                button134.Enabled = true;
                button135.Enabled = true;
                using (new CenterWinDialog(this))
                    MessageBox.Show("Normal.dot backed up to: " + NORMAL_DOTM_BACKUP);
            }
        }

        private void button132_Click(object sender, EventArgs e)
        {
            
            backupNormal();
            

        }

        private void button135_Click(object sender, EventArgs e)
        {
            deleteNormalBackup();
        }

        private void deleteNormalBackup()
        {
            if (File.Exists(NORMAL_DOT_BACKUP)) File.Delete(NORMAL_DOT_BACKUP);
            if (File.Exists(NORMAL_DOTM_BACKUP)) File.Delete(NORMAL_DOTM_BACKUP);

            button134.Enabled = false;
            button135.Enabled = false;
        }

        private void button134_Click(object sender, EventArgs e)
        {
            restoreNormal();
        }

        private void restoreNormal()
        {
            deleteNormal();

            if (File.Exists(NORMAL_DOT_BACKUP))
            {
                backupFile(NORMAL_DOT_BACKUP, NORMAL_DOT);
                button132.Enabled = true;
                button133.Enabled = true;
            }

            if (File.Exists(NORMAL_DOTM_BACKUP))
            {
                backupFile(NORMAL_DOTM_BACKUP, NORMAL_DOTM);
                button132.Enabled = true;
                button133.Enabled = true;

            }
        }
        
        private void button133_Click(object sender, EventArgs e)
        {
            deleteNormal();
        }

        private void deleteNormal()
        {
            if (File.Exists(NORMAL_DOT)) File.Delete(NORMAL_DOT);
            if (File.Exists(NORMAL_DOTM)) File.Delete(NORMAL_DOTM);

            button132.Enabled = false;
            button133.Enabled = false;

        }

        private void button136_Click(object sender, EventArgs e)
        {
            string url = "http://malwarebytes.org/mbam-download-exe-random.php";
            string file = "mbam-rules.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button137_Click(object sender, EventArgs e)
        {
            string url = "http://data.mbamupdates.com/tools/mbam-rules.exe";
            string file = "mbinstall.exe";

            runFromCacheOrDownload(file, url);
        }

        private void cache_age_changed(object sender, EventArgs e)
        {
            Properties.Settings.Default.CacheAgeInDays = this.CacheAge.Value;
            Properties.Settings.Default.Save();
        }

        private void button138_Click(object sender, EventArgs e)
        {
            string url = "http://download.bleepingcomputer.com/sUBs/dds.scr";
            string file = "dds.scr";

            runFromCacheOrDownload(file, url);
        }

        private void button139_Click(object sender, EventArgs e)
        {
            string url = "http://www.kernelmode.info/ARKs/RKUnhookerLE.EXE";
            string file = "RKUnhookerLE.EXE";

            runFromCacheOrDownload(file, url);
        }

        private void button140_Click(object sender, EventArgs e)
        {
            string url = "http://images.malwareremoval.com/random/RSIT.exe";
            string file = "RSIT.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button121_Click_1(object sender, EventArgs e)
        {
            string url = "http://screen317.spywareinfoforum.org/SecurityCheck.exe";
            string file = "SecurityCheck.exe";

            runFromCacheOrDownload(file, url);
        }

        private void linkLabel30_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://sites.google.com/site/rootrepeal/");
        }

        private void button138_Click_1(object sender, EventArgs e)
        {
            string url = "http://jpshortstuff.247fixes.com/GooredFix.exe";
            string file = "GooredFix.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button141_Click(object sender, EventArgs e)
        {
            string url = "http://www.atribune.org/ccount/click.php?id=4";
            string file = "VundoFix.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button142_Click(object sender, EventArgs e)
        {
            string url = "http://secured2k.home.comcast.net/tools/VirtumundoBeGone.exe";
            string file = "VirtumundoBeGone.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button143_Click(object sender, EventArgs e)
        {
            string url = "http://www.trendmicro.com/ftp/products/online-tools/cwshredder.exe";
            string file = "cwshredder.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button144_Click(object sender, EventArgs e)
        {
            helpButton.Capture = false;
            SendMessage(this.Handle, WM_SYSCOMMAND, (IntPtr)SC_CONTEXTHELP, IntPtr.Zero);

        }

        private void button144_Click_1(object sender, EventArgs e)
        {
            string url = "http://www.spybotupdates.biz/files/filealyz-2.0.3.50.exe";
            string file = "filealyz.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button145_Click(object sender, EventArgs e)
        {
            string url = @"http://downloadcenter.mcafee.com/products/tools/foundstone/fport.zip";
            string file = @"Fport-2.0/Fport.exe";
            string zip = "fprot.zip";

            runFromCacheOrDownloadZipfile(file, url, zip, true);
        }

        private void button146_Click(object sender, EventArgs e)
        {
            string url = @"http://www.xblock.com/download/xraypc.zip";
            string file = @"x-raypc.exe";
            string zip = "xraypc.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button147_Click(object sender, EventArgs e)
        {
            string url = @"http://sites.google.com/site/rootrepeal/RootRepeal.zip";
            string file = @"RootRepeal.exe";
            string zip = "RootRepeal.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);

        }

        private void button148_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("SYSTEMROOT") + @"\system32\config\systemprofile\");
        }

        private void button149_Click(object sender, EventArgs e)
        {
            Process.Start(System.Environment.GetEnvironmentVariable("SYSTEMROOT") + @"\system32\config\");
        }

        private void button150_Click(object sender, EventArgs e)
        {
            SAProgressBar pb = new SAProgressBar(this);
            pb.Show();

            
            pb.progressUpdate("Stopping Windows Update server...", 10);
            ProcessRunner.runAndCapture("net.exe", "stop wuauserv");

            pb.progressUpdate("Registering wuapi.dll", 20);
            ProcessRunner.runAndCapture("regsvr32", "/s wuapi.dll");
                        
            pb.progressUpdate("Registering wups.dll", 25);
            ProcessRunner.runAndCapture("regsvr32", "/s wups.dll");

            pb.progressUpdate("Registering wuaueng.dll", 30);
            ProcessRunner.runAndCapture("regsvr32", "/s wuaueng.dll");

            pb.progressUpdate("Registering wuaueng1.dll", 35);
            ProcessRunner.runAndCapture("regsvr32", "/s wuaueng1.dll");

            pb.progressUpdate("Registering wucltui.dll", 40);
            ProcessRunner.runAndCapture("regsvr32", "/s wucltui.dll");

            pb.progressUpdate("Registering wuweb.dll", 45);
            ProcessRunner.runAndCapture("regsvr32", "/s wuweb.dll");

            pb.progressUpdate("Registering jscript.dll", 50);
            ProcessRunner.runAndCapture("regsvr32", "/s jscript.dll");

            pb.progressUpdate("Registering atl.dll", 60);
            ProcessRunner.runAndCapture("regsvr32", "/s atl.dll");

            pb.progressUpdate("Registering softpub.dll", 70);
            ProcessRunner.runAndCapture("regsvr32", "/s softpub.dll");

            pb.progressUpdate("Registering msxml3.dll", 80);
            ProcessRunner.runAndCapture("regsvr32", "/s msxml3.dll");

            pb.progressUpdate("Starting Windows Update server...", 90);
            ProcessRunner.runAndCapture("net.exe", "start wuauserv");

            pb.progressUpdate("Done...", 100);

            pb.Dispose();
        }

        private void button151_Click(object sender, EventArgs e)
        {
            startThisService("Wuauserv", false);
        }

        private void button152_Click(object sender, EventArgs e)
        {
            startThisService("Wuauserv", true);
        }

        private void button153_Click(object sender, EventArgs e)
        {
            string destination_folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string destination_file = destination_folder + "\\sa_minidump_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";

            string source_folder = Environment.GetEnvironmentVariable("SYSTEMROOT") + @"\Minidump\";

            try
            {

                string[] crash_files = Directory.GetFiles(source_folder);

                if (crash_files.Length > 0)
                {
                    SAProgressBar pb = new SAProgressBar(this);
                    pb.Show();

                    pb.progressUpdate("Fetching crash data...", 10);

                    using (ZipFile zip = new ZipFile())
                    {
                        foreach (string dump_file in crash_files)
                        {
                            pb.progressUpdate("Saving " + dump_file);
                            zip.AddFile(dump_file);
                        }

                        pb.progressUpdate("Saving zip file...", 95);
                        zip.Save(destination_file);
                    }

                    pb.progressUpdate("Done...", 100);
                    pb.Dispose();
                    pb = null;

                    using (new CenterWinDialog(this))
                        MessageBox.Show(this, "Your crash data was saved in:\r\n\r\n" + destination_file, "Collection Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    using (new CenterWinDialog(this))
                        MessageBox.Show(this, "Congratulations, your computer seems stable!\r\n\r\n Found no recent crash data.", "Collection Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (DirectoryNotFoundException)
            {
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "Congratulations.\r\n\r\nIt appears that your computer has never\r\nexperienced a Blue Screen of Death error.\r\n\r\n Found no crash data at all.", "Collection Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            

            


        }

        
        private void button154_Click(object sender, EventArgs e)
        {
            string temp_log_dir = CACHE + @"Logs\";

            if (!Directory.Exists(temp_log_dir))
                Directory.CreateDirectory(temp_log_dir);

            string appLogFile = temp_log_dir + "sa_applog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".evtx";
            string sysLogFile = temp_log_dir + "sa_syslog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".evtx";

            string destination_file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\sa_eventlog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";

            SAProgressBar pb = new SAProgressBar(this);
            pb.Show();

            pb.progressUpdate("Collecting informaton...");

            try
            {

                pb.progressUpdate("Collecting Application events...", 50);
                exportEventViewerLog("Application", appLogFile);

                pb.progressUpdate("Collecting System events...", 80);
                exportEventViewerLog("System", sysLogFile);
                                
                pb.progressUpdate("Saving log files...", 90);
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(appLogFile, Path.GetFileName(appLogFile));
                    zip.AddFile(sysLogFile, Path.GetFileName(sysLogFile));
                    zip.Save(destination_file);
                }

                pb.progressUpdate("Done...", 100);

                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "The Event Log files have been saved to:\r\n\r\n" + destination_file, "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (PlatformNotSupportedException)
            {
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "Sorry, this feature is only available on Windows Vista SP1 or higher.", "Platform Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                pb.Dispose();
            }


        }

        private void exportEventViewerLog(string logName, string outputFilePath)
        {
            if(File.Exists(outputFilePath))
                File.Delete(outputFilePath);

            EventLogSession els = new EventLogSession();

            try
            {
                els.ExportLogAndMessages(logName,                   //  Log Name to archive
                                    PathType.LogName,               //  Type of Log
                                    "*",                            //  Query selecting all events
                                    outputFilePath,                  //  Where to export the file
                                    false,                          //  Stop the archive if the query is invalid
                                    CultureInfo.CurrentCulture);    //  Culture to archive the events in
            }
            catch (EventLogNotFoundException)
            {
                //
            }
        }

        private void exportEventViewerLogToText(string logName, string outputFilePath)
        {
            EventLog evt = new EventLog(logName);
            evt.MachineName = "."; // local machine

            using (StreamWriter f = new StreamWriter(outputFilePath))
            {
                f.WriteLine(@"EventID, EventType, EventDate, EventMessage");

                foreach (EventLogEntry e in evt.Entries)
                {
                    string line = "";

                    string msg = e.Message.Replace(Environment.NewLine, " ");

                    line += e.InstanceId + ", ";
                    line += e.EntryType + ", ";
                    line += e.TimeGenerated + ", ";
                    line += msg + "";

                    f.WriteLine(line);
                }
            }
        }

        private void button155_Click(object sender, EventArgs e)
        {

            string temp_log_dir = CACHE + @"Logs\";

            if (!Directory.Exists(temp_log_dir))
                Directory.CreateDirectory(temp_log_dir);

            string appLogFile = temp_log_dir + "sa_applog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string sysLogFile = temp_log_dir + "sa_syslog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

            string destination_file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\sa_eventlog_txt_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";

            SAProgressBar pb = new SAProgressBar(this);
            pb.Show();

            pb.progressUpdate("Collecting information...");

            try
            {
                pb.progressUpdate("Collecting Application events...", 50);
                exportEventViewerLogToText("Application", appLogFile);

                pb.progressUpdate("Collecting System events...", 80);
                exportEventViewerLogToText("System", sysLogFile);

                pb.progressUpdate("Saving log files...", 90);
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(appLogFile, ".");
                    zip.AddFile(sysLogFile, ".");
                    zip.Save(destination_file);
                }

                pb.progressUpdate("Done...", 100);

                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "The Event Log files have been saved to:\r\n\r\n" + destination_file, "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (PlatformNotSupportedException)
            {
                using (new CenterWinDialog(this))
                    MessageBox.Show(this, "Sorry, this feature is only available on Windows Vista SP1 or higher.", "Platform Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                pb.Dispose();
            }

            

        }

        private void button156_Click(object sender, EventArgs e)
        {
            using (new CenterWinDialog(this))
            {
                using (RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    string val = (string)myKey.GetValue("Shell");

                    if (val != null)
                    {
                        if (MessageBox.Show("Found a nonstandard Shell value for CURRENT_USER:\r\n\r\n\t" + val + "\r\n\r\nWould you like to remove it?", "Nonstandard Value Found", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        {
                            // remove it
                            myKey.DeleteValue("Shell");
                        }
                    }
                }



                using (RegistryKey myKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    string val = (string)myKey.GetValue("Shell");

                    if (MessageBox.Show("Following Shell value found for LOCAL_MACHINE:\r\n\r\n\t" + val + "\r\n\r\nWould you like to change it to Explorer.exe?", "Found Shell Value", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // change it
                        myKey.SetValue("Shell", "Explorer.exe", RegistryValueKind.String);
                    }

                }
            }


        }

        private void button157_Click(object sender, EventArgs e)
        {
            string url = "http://the.earth.li/~sgtatham/putty/latest/x86/putty.exe";
            string file = "putty.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button158_Click(object sender, EventArgs e)
        {
            string url = "http://the.earth.li/~sgtatham/putty/latest/x86/puttytel.exe";
            string file = "puttytel.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button159_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("telnet");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Telnet does not seem to be enabled. Try PuTTYtel.", "Telnet not Enabled", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button160_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Local Settings\Application Data\Identities\");
            }
            catch
            {
                using (new CenterWinDialog(this)) { MessageBox.Show(this, "Outlook Express was probably never configured.", "Compatibility Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void button161_Click(object sender, EventArgs e)
        {
            string url = "http://go.microsoft.com/?linkid=9646978";
            string file = "MicrosoftFixit50195.msi";

            runFromCacheOrDownload(file, url);
        }

        private void button162_Click(object sender, EventArgs e)
        {
            //string url = "http://go.microsoft.com/?linkid=9708413";
            string url = "http://go.microsoft.com/?linkid=9708413";
            string file = "MicrosoftFixit.IEAddon.Run.exe";
            
            runFromCacheOrDownload(file, url);
        }

        private void button163_Click(object sender, EventArgs e)
        {
            string url = "http://go.microsoft.com/?linkid=9643543";
            string file = "MicrosoftFixit50210.msi";

            runFromCacheOrDownload(file, url);
        }

        private void button164_Click(object sender, EventArgs e)
        {
            //SAProgressBar pb = new SAProgressBar(this);
            //pb.Show();

            //string output = "Log Generated at: " + DateTime.Now.ToLongDateString() + " " +  DateTime.Now.ToLongTimeString() + "\r\n\r\n";

            //pb.progressUpdate("Checking User Startup Folder", 10);
            //output += "\r\n########################################################\r\n";
            //output += "\t USER STARTUP FOLDER: \r\n";
            //output += "########################################################\r\n\r\n";
            //output += ProcessRunner.runAndCapture("cmd", "/c @tree \"" + Environment.GetEnvironmentVariable("USERPROFILE") + "\\Start Menu\\Programs\\Startup\" /F /A");

            //pb.progressUpdate("Checking All Users Startup Folder", 20);
            //output += "\r\n########################################################\r\n";
            //output += "\t ALL USERS STARTUP FOLDER: \r\n";
            //output += "########################################################\r\n\r\n";
            //output += ProcessRunner.runAndCapture("cmd", "/c @tree \"" + Environment.GetEnvironmentVariable("USERPROFILE") + "\\Start Menu\\Programs\\Startup\" /F /A");

            //pb.progressUpdate("Checking Spooler Folder", 30);
            //output += "\r\n########################################################\r\n";
            //output += "\t SPOOLER FOLDER: \r\n";
            //output += "########################################################\r\n\r\n";
            //output += ProcessRunner.runAndCapture("cmd", "/c @tree \"" + Environment.GetEnvironmentVariable("WINDIR") + "\\system32\\spool\\PRINTERS\" /F /A");

            //pb.progressUpdate("Checking Minidump Folder", 80);
            //output += "\r\n########################################################\r\n";
            //output += "\t MINIDUMP FOLDER: \r\n";
            //output += "########################################################\r\n\r\n";
            //output += ProcessRunner.runAndCapture("cmd", "/c @tree \"" + Environment.GetEnvironmentVariable("WINDIR") + "\\Minidump\" /F /A");


            //pb.progressUpdate("Checking Application Data", 90);
            //output += "\r\n########################################################\r\n";
            //output += "\t APPDATA FOLDER: \r\n";
            //output += "########################################################\r\n\r\n";
            //output += ProcessRunner.runAndCapture("cmd", "/c @tree \"" + Environment.GetEnvironmentVariable("APPDATA") + "\" /F /A");

            //pb.progressUpdate("Checking C:\\", 95);
            //output += "\r\n########################################################\r\n";
            //output += "\t SYSTEM DRIVE DIRECTORIES: \r\n";
            //output += "########################################################\r\n\r\n";
            //output += ProcessRunner.runAndCapture("cmd", "/c @dir \"" + Environment.GetEnvironmentVariable("HOMEDRIVE") + "\\\"");


            //pb.progressUpdate("Done...", 100);
            //pb.Dispose();

            SystemSnooper snp = new SystemSnooper();
            snp.snoop();

            InformationBox f = new InformationBox(snp.Log, "Interesting Folders");
            f.ShowDialog(this);



        }

        private void button168_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\"))
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\");
            else
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\");
        }

        private void button165_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\"))
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\OFFICE11");
            else
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\OFFICE11");
        }

        private void button166_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\"))
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\OFFICE12");
            else
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\OFFICE12");
        }

        private void button167_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\"))
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") + @"\Common Files\Microsoft Shared\OFFICE10");
            else
                Process.Start(System.Environment.GetEnvironmentVariable("PROGRAMFILES") + @"\Common Files\Microsoft Shared\OFFICE10");
        }

        private void button169_Click(object sender, EventArgs e)
        {
            string url = "http://download.piriform.com/ccsetup307.exe";
            string file = "ccsetup307.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button170_Click(object sender, EventArgs e)
        {
            string url = "http://download.mcafee.com/products/licensed/cust_support_patches/MCPR.exe";
            string file = "MCPR.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button171_Click(object sender, EventArgs e)
        {
            string url = "http://go.microsoft.com/?linkid=9665683";
            string file = "MicrosoftFixit50202.msi";

            runFromCacheOrDownload(file, url);
        }

        private void button172_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=46&SrcFamilyId=1B286E6D-8912-4E18-B570-42470E2F3582&SrcDisplayLang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2fa%2f8%2f7%2fa87b3d05-cd04-4743-a23b-b16645e075ac%2fUPHClean-Setup.msi";
            string file = "UPHClean-Setup.msi";

            runFromCacheOrDownload(file, url);
        }

        private void button173_Click(object sender, EventArgs e)
        {
            Form cdk = new CDKeyMgmt();
            cdk.ShowDialog(this);
        }

        private void button174_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=941b3470-3ae9-4aee-8f43-c6bb74cd1466&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f9%2f2%2f2%2f9222D67F-7630-4F49-BD26-476B51517FC1%2fFileFormatConverters.exe";
            string file = "FileFormatConverters.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button175_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=f5539a90-dc41-4792-8ef8-f4de62ff1e81&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f1%2f6%2fb%2f16ba60f5-d478-4d22-a695-203003494477%2fvstor.exe";
            string file = "vstor.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button176_Click(object sender, EventArgs e)
        {
            string url = @"http://www.publicshareware.de/download/PublicFixSearchFolders.zip";
            string file = @"PublicFixSearchFolders.exe";
            string zip = "PublicFixSearchFolders.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button177_Click(object sender, EventArgs e)
        {
            string url = @"http://www.biblet.com/Download/WMDecode.zip";
            string file = @"WMDecode.exe";
            string zip = "WMDecode.zip";

            Process.Start(CACHE);
            
            using (new CenterWinDialog(this))
            {
                
                MessageBox.Show(this, "Please drop the winmail.dat file anywhere in Setup Assistant Cache folder then press OK.", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                //OpenFileDialog openFileDialog1 = new OpenFileDialog();

                //if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                //{
                //    string args =  " \"" + openFileDialog1.FileName + "\"";

                    string args = "";
                    runFromCacheOrDownloadZipfile(file, url, zip, true, args);
                //}
            }
            
        }

        private void button178_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=3657ce88-7cfa-457a-9aec-f4f827f20cac&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f6%2fa%2f6%2f6a689355-b155-4fa7-ad8a-dfe150fe7ac6%2fwordview_en-us.exe";
            string file = "wordview_en-us.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button179_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=1cd6acf9-ce06-4e1c-8dcf-f33f669dbc3a&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2fe%2fa%2f9%2fea913c8b-51a7-41b7-8697-9f0d0a7274aa%2fExcelViewer.exe";
            string file = "ExcelViewer.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button181_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=f9ed50b0-c7df-4fb8-89f8-db2932e624f7&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2fA%2fB%2f3%2fAB3C56B5-B1B3-41CB-A445-D4FB03F8A1BA%2fvisioviewer.exe";
            string file = "Works632_en-US.msi";

            runFromCacheOrDownload(file, url);
        }

        private void button182_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=bf41401e-70fa-465d-ae2e-cf44dbf05297&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2fa%2fd%2f3%2fad3d08da-d7fa-42a8-919c-4533d8a58029%2fWorks632_en-US.msi";
            string file = "Works632_en-US.msi";

            runFromCacheOrDownload(file, url);
        }

        private void button180_Click(object sender, EventArgs e)
        {
            string url = "http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=048dc840-14e1-467d-8dca-19d2a8fd7485&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2ff%2f5%2fa%2ff5a3df76-d856-4a61-a6bd-722f52a5be26%2fPowerPointViewer.exe";
            string file = "PowerPointViewer.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button183_Click(object sender, EventArgs e)
        {
            string url = "http://oldtimer.geekstogo.com/TFC.exe";
            string file = "TFC.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button184_Click(object sender, EventArgs e)
        {
            string url = "http://oldtimer.geekstogo.com/OTL.exe";
            string file = "OTL.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button185_Click(object sender, EventArgs e)
        {
            string url = @"http://iefaq.info/attachments/133/ie8-rereg.zip";
            string file = @"ie8-rereg/ie8-rereg.cmd";
            string zip = "ie8-rereg.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button186_Click(object sender, EventArgs e)
        {
            string url = @"https://skydrive.live.com/?cid=53e1d37f76f69444&sc=documents&id=53E1D37F76F69444%21526#";
            string file = @"XLCleaner.exe";
            string zip = "XLCleaner.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button186_Click_1(object sender, EventArgs e)
        {
            string message = "";

            string url = @"http://winhelp2002.mvps.org/hosts.txt";
            string file = @"hosts";


            try
            {
                    if (File.Exists(HOSTS_FILE))
                    {
                        if (File.Exists(HOTSTS_BACKUP))
                        {
                            File.SetAttributes(this.HOTSTS_BACKUP, FileAttributes.Normal);
                            deleteHostsBackup();
                        }

                        hostsBackup();
                        File.Delete(HOSTS_FILE);
                        message = "HOSTS backed up to " + HOTSTS_BACKUP + "\r\n\r\n";
                    }

                    //createHostsFile();
                    
                    this.Cursor = Cursors.WaitCursor;

                    Uri uri = new Uri(url);
                    String path = CACHE + file;
                    downloadFileToCache(uri, path);
                    
                    CalculateCacheSize();

                   
                    File.Copy(path, HOSTS_FILE);

                    if (File.Exists(HOSTS_FILE))
                        using (new CenterWinDialog(this))
                        using (new CenterWinDialog(this)) MessageBox.Show(this, "Your HOSTS file was replaced with MVPS HOSTS file. To undo, try restoring from backup or recreating.", "MVPS HOSTS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                    {
                        File.Copy(HOTSTS_BACKUP, HOSTS_FILE, true);
                        using (new CenterWinDialog(this)) { MessageBox.Show(this, "Something went wrong while copying.\r\nHOSTS file restored from backup.\r\nPlease try again later.", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                    
             }
             catch (UnauthorizedAccessException) { using (new CenterWinDialog(this)) { MessageBox.Show(this, "HOSTS file is read only. Set it to Read/Write to perform this action.", "File Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
             catch (WebException we) { using (new CenterWinDialog(this)) { MessageBox.Show(this, WEB_ERROR, "Web Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }  }
             finally { this.Cursor = Cursors.Default; }
            
        }

        private void linkLabel5_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://mvt.mcafee.com/mvt/");
        }

        private void button187_Click(object sender, EventArgs e)
        {
            string url = @"http://downloads.sourceforge.net/project/javara/javara/JavaRa/JavaRa.zip?r=http%3A%2F%2Fsourceforge.net%2Fprojects%2Fjavara%2F&ts=1326245059&use_mirror=superb-sea2";
            string file = @"JavaRa.exe";
            string zip = "JavaRa.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button188_Click(object sender, EventArgs e)
        {
            string url = @"http://download.bleepingcomputer.com/spyware/lspfix.zip";
            string file = @"LSPFix.exe";
            string zip = "lspfix.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button189_Click(object sender, EventArgs e)
        {
            string url = @"http://www.stevengould.org/downloads/cleanup/CleanUp40.exe";
            string file = @"CleanUp40.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button190_Click(object sender, EventArgs e)
        {
            string url = @"ftp://ftp.f-secure.com/anti-virus/tools/fsbl.exe";
            string file = @"fsbl.exe";

            runFromCacheOrDownload(file, url);
        }

        private void button191_Click(object sender, EventArgs e)
        {
            string url = @"http://hcidesign.com/memtest/MemTest.zip";
            string file = @"memtest.exe";
            string zip = "MemTest.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button192_Click(object sender, EventArgs e)
        {
            string url = @"http://www.halfdone.com/Development/UnknownDevices/UnknownDevices.zip";
            string file = @"UnknownDevices.exe";
            string zip = "UnknownDevices.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

        private void button193_Click(object sender, EventArgs e)
        {
            string url = @"http://www.nirsoft.net/utils/ofview.zip";
            string file = @"OpenedFilesView.exe";
            string zip = "ofview.zip";

            runFromCacheOrDownloadZipfile(file, url, zip);
        }

       
        
        //

    }

 


}
