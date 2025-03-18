using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SystemMonitor
{
    public static class ComputerInfo
    {
        public static async Task<ComputerInfoDetails> GetComputerInfoAsync()
        {
            var info = new ComputerInfoDetails
            {
                HostName = Environment.MachineName,
                OperationSystem = Environment.OSVersion.ToString(),
                Platform = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit",
                BuildNumber = Environment.OSVersion.Version.Build.ToString(),
                Version = Environment.OSVersion.Version.ToString(),
                Ram = await GetRamAsync(),
                CPU = await GetCpuAsync(),
                Model = await GetCpuModelAsync(),
                Cores = await GetCpuCoresAsync(),
                NetworkAdapters = await GetNetworkAdaptersAsync(),
                Disks = await GetDisksAsync()
            };

            return info;
        }

        private static async Task<long> GetRamAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            return Convert.ToInt64(obj["TotalVisibleMemorySize"]) / 1024;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RAM ma'lumotlarini olishda xatolik: {ex.Message}");
                }
                return 0;
            });
        }

        private static async Task<string> GetCpuAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            return obj["Name"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CPU haqida ma'lumot olishda xatolik: {ex.Message}");
                }
                return "Noma'lum";
            });
        }

        private static async Task<string> GetCpuModelAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            return obj["Caption"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CPU modeli haqida ma'lumot olishda xatolik: {ex.Message}");
                }
                return "Noma'lum";
            });
        }

        private static async Task<int> GetCpuCoresAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            return Convert.ToInt32(obj["NumberOfCores"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CPU yadrolari haqida ma'lumot olishda xatolik: {ex.Message}");
                }
                return 0;
            });
        }

        private static async Task<List<AdapterDetails>> GetNetworkAdaptersAsync()
        {
            return await Task.Run(() =>
            {
                var adapters = new List<AdapterDetails>();
                try
                {
                    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up &&
                            (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                             nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                        {
                            var adapter = new AdapterDetails
                            {
                                NicName = nic.Name,
                                IpAddress = nic.GetIPProperties().UnicastAddresses.FirstOrDefault()?.Address.ToString(),
                                MacAddress = string.Join(":", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2"))),
                                Available = nic.OperationalStatus.ToString()
                            };
                            adapters.Add(adapter);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tarmoq adapterlari haqida ma'lumot olishda xatolik: {ex.Message}");
                }
                return adapters;
            });
        }

        private static async Task<List<DiskDetails>> GetDisksAsync()
        {
            return await Task.Run(() =>
            {
                var disks = new List<DiskDetails>();
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var disk = new DiskDetails
                            {
                                DriveName = obj["DeviceID"].ToString(),
                                TotalSize = ConvertBytesToMB(Convert.ToInt64(obj["Size"])),
                                AvailableSpace = ConvertBytesToMB(Convert.ToInt64(obj["FreeSpace"])) 
                            };
                            disks.Add(disk);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Disklar haqida ma'lumot olishda xatolik: {ex.Message}");
                }
                return disks;
            });
        }

        private static long ConvertBytesToMB(long bytes)
        {
            return bytes / (1024 * 1024);
        }
    }
}
