using Aerocat.Client;
using System.Windows;

namespace Aerocat.Client
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";
            var signalRService = (Application.Current as App).SignalRService;
            bool success = await signalRService.RegisterAsync(UsernameTextBox.Text, PasswordBox.Password);

            if (success)
            {
                MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Registration failed. Username may already be taken.";
            }
        }

        private void SwitchToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}