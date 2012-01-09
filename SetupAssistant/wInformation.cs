using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;
using System.Management;

using System.Windows.Forms;

namespace wInformation
{

    public class wSystemInformation
    {


        private double cpuSpeed;
        private string cpuType, cpuName, cpuManufacturer, dataWidth;
        private string totalDiskSize;

        private string osversion, servicepack, serialnumber, systemdevice, systemdirectory;

        private string defaultgateway;

        private List<wNetworkInformation> networks;

        private string dellServiceTag;

        private string memory;

        private string manufacturer;
        private string model;
        private string name;

        private string cpuClock;

        private string av_suite;
        private string av_uptodate;
        private string av_state;


        public wSystemInformation()
        {
            // get cpu info from registry
            RegistryKey RegKey = Registry.LocalMachine;
            RegKey = RegKey.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");

            Object cpuSpeed = RegKey.GetValue("~MHz");
            Object cpuType = RegKey.GetValue("VendorIdentifier");

            this.cpuSpeed = Double.Parse(cpuSpeed.ToString()) / 1000;
            this.cpuType = cpuType.ToString();

            // get disk size
            ManagementObject diskSize = new ManagementObject("win32_logicaldisk.deviceid=\"C:\"");
            diskSize.Get();
            double hd_size = Double.Parse(diskSize["size"].ToString());
            hd_size /= 1073741824; // convert to Gb

            totalDiskSize = String.Format("{0:0.##}", hd_size) + " GB";

            // get bios serial
            dellServiceTag = "";


            try
            {
                ManagementClass wmi = new ManagementClass("Win32_Bios");
                foreach (ManagementObject bios in wmi.GetInstances())
                {
                    dellServiceTag = bios.Properties["Serialnumber"].Value.ToString().Trim();
                }
            }
            catch { }


            // get total physical memory
            string mem = "0";
            manufacturer = "";
            model = "";
            name = "";

            // objects for making WMI calls
            ManagementObjectSearcher objSearcher;
            ManagementObjectCollection objColl;



            try
            {

                // make a WMI query for generic system info
                objSearcher = new ManagementObjectSearcher("SELECT totalphysicalmemory, manufacturer, model, name FROM Win32_ComputerSystem");
                objColl = objSearcher.Get();
                foreach (ManagementObject mgtObject in objColl)
                {
                    mem = mgtObject["totalphysicalmemory"].ToString();
                    manufacturer = mgtObject["manufacturer"].ToString();
                    model = mgtObject["model"].ToString();
                    name = mgtObject["name"].ToString();
                }
            }
            catch { }



            // calculate physical memory
            double memory = Double.Parse(mem);

            if (memory > 1073741824)
                this.memory = String.Format("{0:0.##}", memory / 1073741824) + " GB"; // in GB
            else
                this.memory = String.Format("{0:0.##}", memory /= 1048576) + " MB";    // in MB

            cpuClock = "";
            dataWidth = "";
            cpuName = "";
            cpuManufacturer = "";

            try
            {

                // find processor info via WMI
                objSearcher = new ManagementObjectSearcher("SELECT maxclockspeed, datawidth, name, manufacturer FROM Win32_Processor");
                objColl = objSearcher.Get();
                foreach (ManagementObject mgtObject in objColl)
                {
                    cpuClock = mgtObject["maxclockspeed"].ToString();

                    dataWidth = mgtObject["datawidth"].ToString();
                    cpuName = mgtObject["name"].ToString();
                    cpuManufacturer = mgtObject["manufacturer"].ToString();
                    //l2CacheSize = mgtObject["l2cachesize"].ToString();

                }
            }
            catch { }

            osversion = "";
            servicepack = "";
            serialnumber = "";
            systemdevice = "";
            systemdirectory = "";

            try
            {

                objSearcher = new ManagementObjectSearcher("SELECT caption, csdversion, serialnumber, systemdevice, windowsdirectory FROM Win32_OperatingSystem");
                objColl = objSearcher.Get();
                foreach (ManagementObject mgtObject in objColl)
                {
                    osversion = mgtObject["caption"].ToString();
                    servicepack = mgtObject["csdversion"].ToString();
                    serialnumber = mgtObject["serialnumber"].ToString();
                    systemdevice = mgtObject["systemdevice"].ToString();
                    systemdirectory = mgtObject["windowsdirectory"].ToString();

                }
            }
            catch { }


            av_suite = "";
            av_uptodate = "";
            av_state = "";

            try
            {


                ConnectionOptions _connectionOptions = new ConnectionOptions();
                _connectionOptions.EnablePrivileges = true;
                _connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                ManagementScope _managementScope = new ManagementScope(string.Format("\\\\{0}\\root\\SecurityCenter2", "localhost"), _connectionOptions);
                _managementScope.Connect();
                ObjectQuery _objectQuery = new ObjectQuery("SELECT * FROM AntivirusProduct");
                ManagementObjectSearcher _managementObjectSearcher = new ManagementObjectSearcher(_managementScope, _objectQuery);
                ManagementObjectCollection _managementObjectCollection = _managementObjectSearcher.Get();
                if (_managementObjectCollection.Count > 0)
                {
                    foreach (ManagementObject item in _managementObjectCollection)
                    {
                        av_suite = item["displayName"].ToString();
                        av_state = item["productState"].ToString();
                    }

                }

            }
            catch { }


            networks = new List<wNetworkInformation>();

            try
            {

                objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
                objColl = objSearcher.Get();

                wNetworkInformation temp;
                temp = new wNetworkInformation();


                foreach (ManagementObject mgtObject in objColl)
                {
                    // we grab only active network interfaces (IP Enabled)
                    if ((bool)mgtObject["ipenabled"])
                    {
                        temp = new wNetworkInformation();

                        temp.Description = mgtObject["description"].ToString();
                        temp.IPAddressArray = (String[])mgtObject["ipaddress"];
                        temp.SubnetAddressArray = (String[])mgtObject["ipsubnet"];
                        temp.DnsServersArray = (String[])mgtObject["dnsserversearchorder"];
                        temp.DefaultGatewayArray = (String[])mgtObject["defaultipgateway"];
                        temp.MacAddress = mgtObject["macaddress"].ToString();

                        if ((bool)mgtObject["dhcpenabled"])
                        {
                            temp.DhcpEnabled = true;
                            temp.DhcpServer = mgtObject["dhcpserver"].ToString();
                        }
                        else
                            temp.DhcpEnabled = false;

                        //temp.DefaultGatewayArray = (string[]) mgtObject["DefaultIPGateway"];

                        networks.Add(temp);
                    }

                }

            }
            catch(Exception e) { MessageBox.Show(e.StackTrace.ToString()); }

        }

        public string ComputerName { get { return Environment.MachineName; } }

        public string OSVersion { get { return osversion; } }
        public string ServicePack { get { return servicepack; } }
        public string SerialNumber { get { return serialnumber; } }
        public string SystemDrive { get { return systemdevice; } }
        public string WindowsDirectory { get { return systemdirectory; } }

        public string Username { get { return Environment.UserName; } }
        public string DomainName { get { return Environment.UserDomainName; } }
        public string DotNetVersion { get { return Environment.Version.ToString(); } }

        public string AntivirusName { get { return av_suite; } }
        public string AntivirusUpToDate { get { return av_uptodate; } }
        public string AntivirusState { get { return av_state; } }

        public string Uptime
        {
            get
            {
                int uptime = Environment.TickCount; // in ms

                // Calculate uptime

                int hours = (uptime / 3600000); // 3600000 ms in an hour
                int minutes = ((uptime % 3600000) / 60000); //  
                int seconds = ((uptime % 3600000) % 60000) / 1000;

                return hours + " hours " + minutes + " minutes " + seconds + " seconds";
            }
        }

        public int CpuCount { get { return Environment.ProcessorCount; } }

        public double CpuSpeed { get { return cpuSpeed; } }

        public string CpuName { get { return cpuName; } }
        public string CpuManufacturer { get { return cpuManufacturer; } }
        public string DataWidth { get { return dataWidth; } }

        public string CpuType { get { return cpuType; } }
        public string TotalDiskSize { get { return totalDiskSize; } }

        public string ServiceTag { get { return dellServiceTag; } }
        public string Manufacturer { get { return manufacturer; } }
        public string Model { get { return model; } }


        public string TotalPhysicalMemory { get { return memory; } }

       
        public string NetworkInformation
        {
            get
            {
                string output = "Network Interfaces: \t" + networks.Count + "\r\n";

              

                foreach (wNetworkInformation n in networks)
                {
                    if(n != null)
                        output += n;

                }

                return output;
            }
        }

        public List<wNetworkInformation> wNetworkInformation
        {
            get { return networks; }
        }

                
    }

    public class wNetworkInformation
    {
        private String[] defaultgateways, dns_server_search_order, ipaddress, subnet;
        private string description;

        private bool dhcpenabled;

        private string macaddress, dhcpserver;

        public wNetworkInformation()
        {

        }

        
        public string Description { get { return description; } set { description = value;  } }
        public String[] IPAddressArray { get { return ipaddress; } set { ipaddress = value; } }
        public String[] SubnetAddressArray { get { return subnet; } set { subnet = value; } }
        public String[] DefaultGatewayArray { get { return defaultgateways; } set { defaultgateways = value; } }
        public String[] DnsServersArray { get { return dns_server_search_order; } set { dns_server_search_order = value; } }
        public string MacAddress { get { return macaddress; } set { macaddress = value; } }
        public string DhcpServer { get { return dhcpserver; } set { dhcpserver = value; } }

        public bool DhcpEnabled { get { return dhcpenabled; } set { dhcpenabled = value;  } }


        public override string ToString()
        {
            string output = "";

            output += "\r\n\t Description: \t " + description + "\r\n";
            output += "\t MAC Address: \t" + macaddress + "\r\n";

            foreach (string ip in ipaddress)
                output += "\t IP Address: \t" + ip +"\r\n";

            foreach (string sub in subnet)
                output += "\t Subnet Address: \t" + sub + "\r\n";

           if(defaultgateways != null)
               foreach (string gateway in defaultgateways)
                    output += "\t Default Gateway: \t" + gateway + "\r\n";

           if (dns_server_search_order != null)
                foreach (string dnsserver in dns_server_search_order)
                    output += "\t DNS Server: \t" + dnsserver + "\r\n";
            

            if(dhcpenabled)
                output += "\t DHCP Server: \t" + dhcpserver + "\r\n";

           
            return output;
        }

    }
}
