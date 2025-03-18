using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using SystemMonitor.Models;

namespace SystemMonitor
{
    public class ProgramManager
    {
        public static List<ProgramDetails> GetInstalledPrograms()
        {
            var programs = new List<ProgramDetails>();
            var seenPrograms = new HashSet<string>(); // Takrorlanganlarni filtrlash uchun

            string[] registryKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var keyPath in registryKeys)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null) continue;

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            string name = subKey?.GetValue("DisplayName")?.ToString();
                            if (string.IsNullOrEmpty(name)) continue;

                            // Dastur allaqachon qo‘shilgan bo‘lsa, tashlab ketamiz
                            if (seenPrograms.Contains(name)) continue;
                            seenPrograms.Add(name); // Dastur nomini ro‘yxatga kiritamiz

                            string uninstallString = subKey?.GetValue("UninstallString")?.ToString();
                            if (string.IsNullOrEmpty(uninstallString)) continue;

                            bool isSystemComponent = subKey?.GetValue("SystemComponent") is int systemComponent && systemComponent == 1;
                            if (isSystemComponent) continue;

                            string version = subKey?.GetValue("DisplayVersion")?.ToString();
                            string installLocation = subKey?.GetValue("InstallLocation")?.ToString();
                            string installDate = subKey?.GetValue("InstallDate")?.ToString();

                            programs.Add(new ProgramDetails
                            {
                                Name = name,
                                Size = GetProgramSize(installLocation),
                                Type = "User",
                                InstalledDate = ParseInstallDate(installDate),
                                Version = version
                            });
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
            if (string.IsNullOrEmpty(installLocation) || !Directory.Exists(installLocation))
                return null;

            long size = 0;
            try
            {
                foreach (var file in Directory.GetFiles(installLocation, "*.*", SearchOption.AllDirectories))
                {
                    size += new FileInfo(file).Length;
                }
                return (int?)(size / 1024 / 1024); // MB ga o'tkazish
            }
            catch
            {
                return null;
            }
        }
    }
}
