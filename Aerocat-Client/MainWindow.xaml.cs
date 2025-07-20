// In project: Aerocat.Client
// File: MainWindow.xaml.cs
using Aerocat.Client.Services;
using Aerocat.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Aerocat.Client
{
    // A view model for the contact list items to handle status colors
    public class ContactViewModel : UserViewModel
    {
        public SolidColorBrush StatusColor
        {
            get
            {
                return Status switch
                {
                    UserStatus.Online => Brushes.LimeGreen,
                    UserStatus.Away => Brushes.Orange,
                    UserStatus.Busy => Brushes.Red,
                    _ => Brushes.Gray,
                };
            }
        }
        public string DisplayName => Username;
    }

    public partial class MainWindow : Window
    {
        private readonly SignalRService _signalRService;
        public ObservableCollection<ContactViewModel> OnlineContacts { get; set; }
        public ObservableCollection<ContactViewModel> OfflineContacts { get; set; }

        // Store open chat windows
        private readonly Dictionary<string, ChatWindow> _openChatWindows = new Dictionary<string, ChatWindow>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            OnlineContacts = new ObservableCollection<ContactViewModel>();
            OfflineContacts = new ObservableCollection<ContactViewModel>(); // For future use
            OnlineContactsList.ItemsSource = OnlineContacts;
            OfflineContactsList.ItemsSource = OfflineContacts;

            _signalRService = (Application.Current as App).SignalRService;

            // Subscribe to events from the SignalR service
            _signalRService.OnUserStatusChanged += OnUserStatusChanged;
            _signalRService.OnMessageReceived += OnMessageReceived;

            this.Loaded += MainWindow_Loaded;
            OnlineContactsList.MouseDoubleClick += ContactList_MouseDoubleClick;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserDisplayName.Text = _signalRService.LoggedInUsername;

            // Get initial list of online users
            var onlineUsers = await _signalRService.GetOnlineUsersAsync();
            foreach (var user in onlineUsers)
            {
                if (user.Username != _signalRService.LoggedInUsername) // Don't add yourself to the list
                {
                    OnlineContacts.Add(new ContactViewModel { Username = user.Username, Status = user.Status });
                }
            }
            UpdateContactCounts();
        }

        private void OnUserStatusChanged(UserViewModel user)
        {
            // This needs to run on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existingContact = OnlineContacts.FirstOrDefault(c => c.Username == user.Username);

                if (user.Status == UserStatus.Online)
                {
                    if (existingContact == null && user.Username != _signalRService.LoggedInUsername)
                    {
                        OnlineContacts.Add(new ContactViewModel { Username = user.Username, Status = UserStatus.Online });
                    }
                }
                else // User went offline
                {
                    if (existingContact != null)
                    {
                        OnlineContacts.Remove(existingContact);
                        // You could move them to the OfflineContacts list here
                    }
                }
                UpdateContactCounts();
            });
        }

        private void OnMessageReceived(Message message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var chatPartnerUsername = message.SenderUsername == _signalRService.LoggedInUsername ? message.ReceiverUsername : message.SenderUsername;

                if (_openChatWindows.TryGetValue(chatPartnerUsername, out var chatWindow))
                {
                    chatWindow.AddMessage(message);
                }
                else
                {
                    // If chat window is not open, open it
                    var newChatWindow = new ChatWindow(chatPartnerUsername);
                    _openChatWindows.Add(chatPartnerUsername, newChatWindow);
                    newChatWindow.Closed += (s, e) => _openChatWindows.Remove(chatPartnerUsername);
                    newChatWindow.Show();
                    newChatWindow.AddMessage(message);
                }
            });
        }

        private void ContactList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is ContactViewModel selectedContact)
            {
                if (_openChatWindows.ContainsKey(selectedContact.Username))
                {
                    _openChatWindows[selectedContact.Username].Activate();
                }
                else
                {
                    var chatWindow = new ChatWindow(selectedContact.Username);
                    _openChatWindows.Add(selectedContact.Username, chatWindow);
                    chatWindow.Closed += (s, ev) => _openChatWindows.Remove(selectedContact.Username);
                    chatWindow.Show();
                }
            }
        }

        private void UpdateContactCounts()
        {
            OnlineExpander.Header = $"📱 Online ({OnlineContacts.Count})";
            OfflineExpander.Header = $"💤 Offline ({OfflineContacts.Count})";
        }
    }
}