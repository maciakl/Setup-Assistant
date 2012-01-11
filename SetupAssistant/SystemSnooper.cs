using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace SetupAssistant
{
    /**
     *  This class provides information about the system by snooping around the file system 
     *  and registry and making assumptions based on existing files and data.
     *  
     *  It will make guesses as to what type of software is installed on the system by checking 
     *  whether not certain known files and registry entries exist.
     *  
     **/
    class SystemSnooper
    {
        private StringBuilder log;

        private string PROGRAMFILES;
        private string PROGRAMFILESx86;
        private string APPDATA;
        private string USERPROFILE;
        private string WINDIR;

        private string OFFICE10;
        private string OFFICE11;
        private string OFFICE12;
        private string OFFICE14;

        private string CURRENT_PROGRAMFILES;

        private string JAVA;
        private string MALWAREBYTES;
        private string FIREFOX;


        public SystemSnooper()
        {
            log = new StringBuilder();

            log.AppendLine("SystemSnooper (c) 2011 Lukasz Grzegorz Maciak");
            log.AppendLine("=============================================\r\n");
            log.AppendLine("SystemSnooper provides useful information by snooping throuhg system directories and registry.");
            log.AppendLine("The range and scope of collected data will be expanded in future versions of Luke's Setup Assistant.");
            log.AppendLine();

            PROGRAMFILES = Environment.GetEnvironmentVariable("PROGRAMFILES");
            PROGRAMFILESx86 = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)");
            APPDATA = Environment.GetEnvironmentVariable("APPDATA");
            USERPROFILE = Environment.GetEnvironmentVariable("USERPROFILE");
            WINDIR = Environment.GetEnvironmentVariable("WINDIR");


            if (PROGRAMFILESx86 == null)
            {
                log.AppendLine("This appears to be a 32 bit system.");
                log.AppendLine();
                CURRENT_PROGRAMFILES = PROGRAMFILES;
            }
            else
            {
                log.AppendLine("This appears to be a 64 bit system.");
                log.AppendLine();
                CURRENT_PROGRAMFILES = PROGRAMFILESx86;
            }

            OFFICE10 = CURRENT_PROGRAMFILES + @"\Microsoft Office\OFFICE10";
            OFFICE11 = CURRENT_PROGRAMFILES + @"\Microsoft Office\OFFICE11";
            OFFICE12 = CURRENT_PROGRAMFILES + @"\Microsoft Office\Office12";
            OFFICE14 = CURRENT_PROGRAMFILES + @"\Microsoft Office\Office14";

            JAVA = CURRENT_PROGRAMFILES + @"\Java\";

            MALWAREBYTES = CURRENT_PROGRAMFILES + @"\Malwarebytes' Anti-Malware";
            FIREFOX = CURRENT_PROGRAMFILES + @"\Mozilla Firefox";


        }

        public void snoop()
        {

            if(Directory.Exists(OFFICE10))
            {
                log.AppendLine("Detected Office XP in the default folder.");
                snoopOffice(OFFICE10);
            }

            if (Directory.Exists(OFFICE11))
            {
                log.AppendLine("Detected Office 2003 in the default folder.");
                snoopOffice(OFFICE11);
            }

            if (Directory.Exists(OFFICE12))
            {
                log.AppendLine("Detected Office 2007 in the default folder.");
                snoopOffice(OFFICE12);
            }

            if (Directory.Exists(OFFICE14))
            {
                log.AppendLine("Detected Office 2010 in the default folde.r");
                snoopOffice(OFFICE14);
            }


            if (Directory.Exists(JAVA))
            {
                log.AppendLine("Detected a default JAVA directory.");

                foreach(string d in Directory.GetDirectories(JAVA))
                {
                    log.AppendFormat("\t- {0}\r\n", Path.GetFileName(d));

                }

                log.AppendLine();
            }

            if (Directory.Exists(MALWAREBYTES))
            {
                log.AppendLine("Detected a default Malwarebytes directory.");
                
                if(File.Exists(MALWAREBYTES + @"\mbam.exe")) log.AppendLine("\t - mbam.exe present");

                log.AppendLine();
            }

            if (Directory.Exists(FIREFOX))
            {
                log.AppendLine("Detected a default Firefox directory.");

                if (File.Exists(FIREFOX + @"\firefox.exe")) log.AppendLine("\t - firefox.exe present");
                log.AppendLine();

                if (Directory.Exists(FIREFOX + @"\plugins\"))
                {
                    foreach (string d in Directory.GetFiles(FIREFOX + @"\plugins"))
                        log.AppendFormat("\t - plugin: {0}\r\n", Path.GetFileName(d));
                }

                log.AppendLine();

                string FIREFOXDATA = APPDATA + @"\Mozilla\Firefox";

                if (Directory.Exists(FIREFOXDATA + @"\Profiles"))
                {
                    foreach (string d in Directory.GetDirectories(FIREFOXDATA + @"\Profiles"))
                    {
                        log.AppendFormat("\t - profile: {0}\r\n", Path.GetFileName(d));

                        if(Directory.Exists(d + @"\extensions"))
                            foreach (string e in Directory.GetDirectories(d + @"\extensions"))
                                log.AppendFormat("\t\t - extension: {0}\r\n", Path.GetFileName(e));

                        log.AppendLine();

                    }

                   
                }
            }
            
            
            
        }

        private void snoopOffice(string office_folder)
        {

            string OFFICEDATA = APPDATA + @"\Microsoft";

            if (File.Exists(office_folder + @"\WINWORD.EXE")) log.AppendLine("\t- Word");
            {
                // word templates
                if (Directory.Exists(OFFICEDATA + @"\Templates"))
                    foreach (string e in Directory.GetFiles(OFFICEDATA + @"\Templates"))
                        if (e.ToLower().EndsWith(".dot") || e.ToLower().EndsWith(".dotm"))
                            log.AppendFormat("\t\t - template: {0}\r\n", Path.GetFileName(e));

                log.AppendLine();

                // word backup / recovery files
                string WORDDATA = OFFICEDATA + @"\Word";

                if (Directory.Exists(WORDDATA))
                    foreach (string e in Directory.GetFiles(WORDDATA))
                        if (e.ToLower().EndsWith(".wbk") || e.ToLower().EndsWith(".asd"))
                            log.AppendFormat("\t\t - backup: {0}\r\n", Path.GetFileName(e));



                // word startup items

            }

            if (File.Exists(office_folder + @"\EXCEL.EXE")) log.AppendLine("\t- Excel");

            // excel ui files

            // excel startup items


            if (File.Exists(office_folder + @"\OUTLOOK.EXE"))
            {
                log.AppendLine("\t- Outlook");

                string OUTLOOKDIR = USERPROFILE + @"\Local Settings\Application Data\Microsoft\Outlook";
                string OUTLOOKDIR2 = APPDATA + @"\Microsoft\Outlook";

                if (Directory.Exists(OUTLOOKDIR))
                    foreach (string e in Directory.GetFiles(OUTLOOKDIR))
                        if (e.ToLower().EndsWith(".pst"))
                            log.AppendFormat("\t\t - pst: {0}\r\n", Path.GetFileName(e));

                if (Directory.Exists(OUTLOOKDIR2))
                    foreach (string e in Directory.GetFiles(OUTLOOKDIR2))
                        if (e.ToLower().EndsWith(".nk2"))
                            log.AppendFormat("\t\t - nk2: {0}\r\n", Path.GetFileName(e));

            }

            if (File.Exists(office_folder + @"\POWERPNT.EXE")) log.AppendLine("\t- Powerpoint");
            if (File.Exists(office_folder + @"\MSACCESS.EXE")) log.AppendLine("\t- Access");

            if (File.Exists(office_folder + @"\ONENOTE.EXE")) log.AppendLine("\t- One Note");
            if (File.Exists(office_folder + @"\FRONTPG.EXE")) log.AppendLine("\t- Front Page");
            if (File.Exists(office_folder + @"\MSPUB.EXE")) log.AppendLine("\t- Publisher");

            log.AppendLine();
        }

        public string Log { get {return log.ToString();} }
    }
}
