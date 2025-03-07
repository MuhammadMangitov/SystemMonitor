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

            StartMonitoring();
            socketManager = new SocketManager();
            _ = socketManager.StartSocketListener();
            _ = GetAndSaveJwtToken();
        }
        public async Task GetAndSaveJwtToken()
        {
            var token = await ApiClient.GetJwtTokenFromApi();

            if (!string.IsNullOrEmpty(token))
            {
                SQLiteHelper.DeleteOldJwtToken();

                SQLiteHelper.InsertJwtToken(token);
                Console.WriteLine("JWT token saqlandi");
            }
            else
            {
                Console.WriteLine("JWT token olishda xatolik");
            }
        }
        private void StartMonitoring()
        {
            
            SendComputerInfo();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(10)
            };
            timer.Tick += async (s, e) => await SendProgramInfo();
            timer.Start();
        }

        private async void SendComputerInfo()
        {
            var info = ComputerInfo.GetComputerInfo();
            await ApiClient.SendComputerInfo(info);
        }

        private async Task SendProgramInfo()
        {
            var programs = ProgramManager.GetRunningPrograms();
            await ApiClient.SendProgramInfo(programs);
        }
    }
}
