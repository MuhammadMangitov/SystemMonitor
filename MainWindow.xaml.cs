using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SystemMonitor.Models;

namespace SystemMonitor
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private SocketManager socketManager;

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;

            LogWrapper.Execute(() =>
            {
                StartMonitoring();
            }, "UI");
        }

        private async void StartMonitoring()
        {
            await LogWrapper.ExecuteAsync(async () =>
            {
                SQLiteHelper.CreateConnection();

                var (token, statusCode) = await ApiClient.GetJwtTokenFromApi();
                if (!string.IsNullOrEmpty(token))
                {
                    SQLiteHelper.InsertJwtToken(token);
                    Console.WriteLine("JWT token saqlandi");

                    if (statusCode == 201)
                    {
                        await SendProgramInfo();
                    }            
                }
                else
                {
                    Console.WriteLine("JWT token olishda xatolik");
                }

                if (SQLiteHelper.ShouldSendProgramInfo())
                {
                    await SendProgramInfo();
                }

                StartTimer();
                socketManager = new SocketManager();
                bool isConnected = await socketManager.StartSocketListener();
                if (isConnected)
                {
                    Console.WriteLine("Socket.io tayyor!");
                }
                else
                {
                    Console.WriteLine("Socket.io ulanmadi, keyinroq qayta urinib ko‘ring.");
                }

            }, "Monitoring");
        }

        private async Task SendProgramInfo()
        {
            await LogWrapper.ExecuteAsync(async () =>
            {
                var programs = ProgramManager.GetInstalledPrograms();
                bool success = await ApiClient.SendProgramInfo(programs);

                if (success)
                {
                    Console.WriteLine("Dasturlar ro‘yxati muvaffaqiyatli jo‘natildi.");
                    SQLiteHelper.UpdateLastSentTime(DateTime.UtcNow); 
                }
                else
                {
                    Console.WriteLine("Dasturlar ro‘yxatini jo‘natishda xatolik yuz berdi.");
                }

            }, "Monitoring");
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(24) 
            };

            timer.Tick += async (sender, args) =>
            {
                Console.WriteLine("24 soat o‘tdi, yangi ma’lumot yuborilmoqda...");
                if (!SQLiteHelper.ShouldSendProgramInfo())
                {
                    Console.WriteLine("24 soat o‘tmaganligi sababli dasturlar ro‘yxati jo‘natilmadi.");
                    return;
                }

                await SendProgramInfo();
            };

            timer.Start();
        }
        public static class LogHelper
        {
            private static readonly string logFilePath = "C:\\Users\\Muhammad\\Desktop\\log.txt";

            public static void WriteLog(string message)
            {
                try
                {
                    string logMessage = $"{DateTime.Now}: {message}";

                    if (!File.Exists(logFilePath))
                    {
                        File.Create(logFilePath).Close();
                    }

                    File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Log yozishda xatolik: {ex.Message}");
                }
            }
        }

    }
}
