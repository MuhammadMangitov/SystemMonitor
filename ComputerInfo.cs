using System;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SystemMonitor.Models;

namespace SystemMonitor
{
    public class ComputerInfo
    {
        public static ComputerInfoDetails GetComputerInfo()
        {
            var info = new ComputerInfoDetails
            {
                username = Environment.UserName,
                computer_name = Environment.MachineName
            };

            // MAC manzil
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    info.mac_address = string.Join(":", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
                    break;
                }
            }

            Parallel.Invoke(
                () => {
                    using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            info.ram = Convert.ToInt64(obj["TotalVisibleMemorySize"]) / 1024; // KB dan MB ga
                        }
                    }
                },
                () => {
                    using (var searcher = new ManagementObjectSearcher("SELECT Size FROM Win32_LogicalDisk WHERE DriveType=3"))
                    {
                        long totalStorage = 0;
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            totalStorage += Convert.ToInt64(obj["Size"]);
                        }
                        info.storage = totalStorage / 1024 / 1024; // Baytdan MB ga
                    }
                }
            );

            return info;
        }
    }
}
