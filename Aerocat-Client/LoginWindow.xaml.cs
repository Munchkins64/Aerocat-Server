// In project: Aerocat.Client
// File: LoginWindow.xaml.cs
using Aerocat.Client;
using System.Windows;

namespace Aerocat.Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";
            var signalRService = (Application.Current as App).SignalRService;
            bool success = await signalRService.LoginAsync(UsernameTextBox.Text, PasswordBox.Password);

            if (success)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Login failed. Check username/password.";
            }
        }

        private void SwitchToRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}