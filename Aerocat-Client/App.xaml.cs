// In project: Aerocat.Client
// File: App.xaml.cs
using Aerocat.Client.Services;
using System.Windows;

namespace Aerocat.Client
{
    public partial class App : Application
    {
        public SignalRService SignalRService { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SignalRService = new SignalRService();
            await SignalRService.ConnectAsync();

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}