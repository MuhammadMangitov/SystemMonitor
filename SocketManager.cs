using SocketIOClient;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SystemMonitor
{
    public class SocketManager
    {
        private readonly SocketIOClient.SocketIO client;
        private bool isRegistered = false;
        private static readonly HashSet<string> AllowedCommands = new HashSet<string>
        {
            "ipconfig", "systeminfo", "whoami", "tasklist"
        };

        public SocketManager()
        {
            client = new SocketIOClient.SocketIO("http://your-backend-url", new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionDelay = 1000,
                ConnectionTimeout = TimeSpan.FromSeconds(10)
            });
        }

        public async Task StartSocketListener()
        {
            client.OnConnected += async (sender, e) =>
            {
                if (!isRegistered)
                {
                    await client.EmitAsync("register", "SystemMonitor_Client");
                    isRegistered = true;
                }
            };

            client.On("execute", async response =>
            {
                string command = response.GetValue<string>();

                if (!AllowedCommands.Contains(command.ToLower()))
                {
                    //await ApiClient.SendCommandResult(command, null, "Permission denied");
                    return;
                }

                var (result, error) = await ExecuteCommand(command);
                //await ApiClient.SendCommandResult(command, result, error);
            });

            await client.ConnectAsync();
        }

        private async Task<(string result, string error)> ExecuteCommand(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Path\To\psexec.exe",
                        Arguments = $"-accepteula {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit(10000)); 

                return (output, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public async Task StopSocketListener()
        {
            if (client != null)
            {
                await client.DisconnectAsync();
            }
        }
    }
}
