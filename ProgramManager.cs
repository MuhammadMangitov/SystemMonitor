using System;
using System.Collections.Generic;
using Microsoft.Win32;
using SystemMonitor.Models;

namespace SystemMonitor
{
    public class ProgramManager
    {
            public static List<ProgramDetails> GetInstalledPrograms()
            {
                var programs = new List<ProgramDetails>();
                string[] registryKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

                foreach (var keyPath in registryKeys)
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null)
                        {
                            foreach (var subKeyName in key.GetSubKeyNames())
                            {
                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    string name = subKey?.GetValue("DisplayName")?.ToString();
                                    string version = subKey?.GetValue("DisplayVersion")?.ToString() ;
                                    string installLocation = subKey?.GetValue("InstallLocation")?.ToString() ;
                                    string installDate = subKey?.GetValue("InstallDate")?.ToString();
                                    bool isSystemComponent = Convert.ToBoolean(subKey?.GetValue("SystemComponent", 0));

                                    var details = new ProgramDetails
                                    {
                                        name = name,
                                        size = GetProgramSize(installLocation),
                                        type = isSystemComponent ? "System" : "User",
                                        installed_date = ParseInstallDate(installDate),
                                        version = version
                                    };

                                    programs.Add(details);

                                    return programs;
                                }
                            }
                        }
                    }
                }

                return programs;
            }
            private static DateTime? ParseInstallDate(string installDate)
            {
                if (DateTime.TryParseExact(installDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    return date;
                }
                return null;
            }

            private static int? GetProgramSize(string installLocation)
            {
                if (string.IsNullOrEmpty(installLocation) || !System.IO.Directory.Exists(installLocation))
                    return null;

                long size = 0;
                var files = System.IO.Directory.GetFiles(installLocation, "*.*", System.IO.SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fileInfo = new System.IO.FileInfo(file);
                    size += fileInfo.Length;
                }

                return (int?)(size / 1024 / 1024); 
            }
        }

    }
