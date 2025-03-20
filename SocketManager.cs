using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Win32;
using SocketIOClient;

namespace SystemMonitor
{
    public class SocketManager
    {
        private readonly SocketIOClient.SocketIO client;
        private bool isRegistered = false;
        private static readonly HttpClient httpClient = new HttpClient();

        public SocketManager()
        {
            string socketUrl = ConfigurationManager.GetSocketServerUrl();
            client = new SocketIOClient.SocketIO(socketUrl, new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = 5,
                ReconnectionDelay = 2000,
                ConnectionTimeout = TimeSpan.FromSeconds(10)
            });

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            client.On("connect", async response =>
            {
                Console.WriteLine("✅ Socket.io muvaffaqiyatli ulandi!");

                if (!isRegistered)
                {
                    await client.EmitAsync("register", "SystemMonitor_Client");
                    isRegistered = true;
                    Console.WriteLine("🔹 Client ro‘yxatga olindi.");
                }
            });

            client.On("delete_app", async response =>
            {
                try
                {
                    var parsedData = response.GetValue<Dictionary<string, object>>();
                    if (parsedData.TryGetValue("name", out var nameObj))
                    {
                        string appName = nameObj.ToString();
                        Console.WriteLine($"O‘chirish uchun qabul qilindi: {appName}");

                        bool result = await UninstallApplicationAsync(appName);
                        string status = result ? "success" : "error";

                        await client.EmitAsync("deleted_app", new { command = "delete_app", status, name = appName });
                        Console.WriteLine($"O‘chirish natijasi: {status}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ DELETE xatosi: {ex.Message}");
                }
            });

            client.On("install_app", async response =>
            {
                try
                {
                    var parsedData = response.GetValue<Dictionary<string, object>>();
                    if (!parsedData.TryGetValue("name", out var filenameObj)) return;

                    string filename = filenameObj.ToString();
                    Console.WriteLine($"Yuklab olish uchun qabul qilindi: {filename}");

                    string jwtToken = await SQLiteHelper.GetJwtToken();
                    if (string.IsNullOrEmpty(jwtToken))
                    {
                        Console.WriteLine("Token topilmadi!");
                        await client.EmitAsync("downloaded_app", new { command = "download_app", status = "error", filename });
                        return;
                    }

                    string apiUrl = ConfigurationManager.GetInstallerApiUrl();
                    string requestUrl = $"{apiUrl}/{filename}";
                    string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{filename}.exe");

                    bool downloaded = await DownloadFileAsync(requestUrl, savePath, jwtToken);
                    string status = downloaded ? "success" : "error";

                    await client.EmitAsync("downloaded_app", new { command = "download_app", status, filename });
                    Console.WriteLine($"{filename} yuklab olish natijasi: {status}");

                    if (!downloaded)
                    {
                        Console.WriteLine("Yuklab olish muvaffaqiyatsiz tugadi!");
                        return;
                    }

                    Console.WriteLine($"O‘rnatish boshlanmoqda: {savePath}");
                    bool installed = await InstallApplicationAsync(savePath);

                    string installStatus = installed ? "installed" : "install_failed";
                    await client.EmitAsync("installed_app", new { command = "install_app", status = installStatus, filename });
                    Console.WriteLine($"{filename} o‘rnatish natijasi: {installStatus}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"INSTALL xatosi: {ex.Message}");
                }
            });

        }

        public async Task<bool> StartSocketListener()
        {
            string jwtToken = await SQLiteHelper.GetJwtToken();
            if (string.IsNullOrEmpty(jwtToken))
            {
                Console.WriteLine("❌ Token topilmadi!");
                return false;
            }

            client.Options.ExtraHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {jwtToken}" } };
            await client.ConnectAsync();
            return client.Connected;
        }

        private async Task<bool> UninstallApplicationAsync(string appName)
        {
            try
            {
                string uninstallString = GetUninstallString(appName);
                if (string.IsNullOrEmpty(uninstallString)) return false;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{uninstallString} /quiet /norestart\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                return await Task.Run(() =>
                {
                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        private string GetUninstallString(string appName)
        {
            string[] registryPaths =
            {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (string path in registryPaths)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key == null) continue;

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey?.GetValue("DisplayName")?.ToString() == appName)
                                return subKey.GetValue("UninstallString")?.ToString();
                        }
                    }
                }
            }

            string userRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey userKey = Registry.CurrentUser.OpenSubKey(userRegistryPath))
            {
                if (userKey != null)
                {
                    foreach (var subKeyName in userKey.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = userKey.OpenSubKey(subKeyName))
                        {
                            if (subKey?.GetValue("DisplayName")?.ToString() == appName)
                                return subKey.GetValue("UninstallString")?.ToString();
                        }
                    }
                }
            }

            return null; 
        }


        private async Task<bool> DownloadFileAsync(string url, string savePath, string jwtToken)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                using (HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("❌ Serverdan fayl yuklab olishda xatolik!");
                        return false;
                    }

                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    return File.Exists(savePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Yuklab olishda xatolik: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> InstallApplicationAsync(string installerPath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/silent /verysilent /norestart",
                    UseShellExecute = true, // 🔹 Windows shell orqali ishga tushirish
                    Verb = "runas" // 🔹 Administrator sifatida ishga tushirish
                };

                using (Process process = Process.Start(psi))
                {
                    await Task.Run(() => process.WaitForExit());
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ O‘rnatish xatosi: {ex.Message}");
                return false;
            }
        }

    }
}
