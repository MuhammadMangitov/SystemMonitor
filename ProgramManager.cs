using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Diagnostics; // FileVersionInfo uchun
using SystemMonitor.Models;

namespace SystemMonitor
{
    public class ProgramManager
    {
        public static List<ProgramDetails> GetRunningPrograms()
        {
            var programs = new List<ProgramDetails>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name, ExecutablePath FROM Win32_Process"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString();
                        string path = obj["ExecutablePath"]?.ToString();

                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                            continue;

                        var details = new ProgramDetails
                        {
                            name = name,
                            executable_path = path
                        };

                        try
                        {
                            var fileInfo = new FileInfo(details.executable_path);
                            details.file_size = fileInfo.Exists ? fileInfo.Length : (long?)null;
                            details.creation_time = fileInfo.Exists ? fileInfo.CreationTime : (DateTime?)null;

                            // Versiyani olish
                            var versionInfo = FileVersionInfo.GetVersionInfo(details.executable_path);
                            details.version = versionInfo.FileVersion;
                        }
                        catch (Exception fileEx)
                        {
                            Console.WriteLine($"[Xatolik] Fayl ma'lumotlarini olishda xatolik: {fileEx.Message}");
                        }

                        programs.Add(details);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Xatolik] Dasturlarni olishda xatolik: {ex.Message}");
            }

            return programs;
        }
    }
}
