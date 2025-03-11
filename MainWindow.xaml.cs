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
                //socketManager = new SocketManager();
                //_ = socketManager.StartSocketListener();
                _ = GetAndSaveJwtToken();
                
            }, "UI");
        }

        public async Task GetAndSaveJwtToken()
        {
            await LogWrapper.ExecuteAsync(async () =>
            {
                var token = await ApiClient.GetJwtTokenFromApi();
                if (!string.IsNullOrEmpty(token))
                {
                    SQLiteHelper.InsertJwtToken(token);
                    Console.WriteLine("JWT token saqlandi");
                }
                else
                {
                    Console.WriteLine("JWT token olishda xatolik");
                }
            }, "API");
        }

        private void StartMonitoring()
        {
            LogWrapper.Execute(() =>
            {
                SendComputerInfo();
                SendProgramInfo();

            }, "Monitoring");
        }

        private void SendComputerInfo()
        {
            LogWrapper.Execute(() =>
            {
                var info = ComputerInfo.GetComputerInfo();
                SQLiteHelper.CreateConnection();
            }, "Monitoring");
        }

        private async void SendProgramInfo()
        {
            await LogWrapper.ExecuteAsync(async () =>
            {
                var programs = ProgramManager.GetInstalledPrograms();
                await ApiClient.SendProgramInfo(programs);
            }, "Monitoring");
        }
    }
}