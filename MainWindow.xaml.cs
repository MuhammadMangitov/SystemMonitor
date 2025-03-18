using System;
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

                    if (statusCode == 200)
                    {
                        await SendProgramInfo();
                        StartTimer(); 
                    }
                }
                else
                {
                    Console.WriteLine("JWT token olishda xatolik");
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
                Interval = TimeSpan.FromMinutes(1) // ⏳ 1 daqiqada bir ishlaydi
            };

            timer.Tick += async (sender, args) =>
            {
                Console.WriteLine("🔄 1 daqiqa o‘tdi, yangi ma’lumot yuborilmoqda...");
                await SendProgramInfo();
            };

            timer.Start();
        }
    }
}
