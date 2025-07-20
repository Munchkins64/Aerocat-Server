// In project: Aerocat.Client
// File: ChatWindow.xaml.cs
using Aerocat.Client.Services;
using Aerocat.Shared;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Aerocat.Client
{
    public partial class ChatWindow : Window
    {
        private readonly SignalRService _signalRService;
        private readonly string _contactUsername;

        // Emoji dictionary
        private static readonly Dictionary<string, string> EmojiMap = new Dictionary<string, string>
        {
            { ":smile:", "😊" }, { ":lol:", "😂" }, { ":sad:", "😢" },
            { ":wink:", "😉" }, { ":heart:", "❤️" }, { ":fire:", "🔥" },
            { ":skull:", "💀" }, { ":thumbsup:", "👍" }
        };

        public ChatWindow(string contactUsername)
        {
            InitializeComponent();
            _contactUsername = contactUsername;
            _signalRService = (Application.Current as App).SignalRService;

            // Setup window title and labels
            this.Title = $"Conversation with {_contactUsername}";
            ContactNameLabel.Text = _contactUsername;

            SendButton.Click += SendButton_Click;
            MessageInput.KeyDown += MessageInput_KeyDown;
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private async void SendMessage()
        {
            var messageContent = MessageInput.Text;
            if (string.IsNullOrWhiteSpace(messageContent)) return;

            await _signalRService.SendMessageAsync(_contactUsername, messageContent);
            MessageInput.Clear();
        }

        public void AddMessage(Message message)
        {
            var isMyMessage = message.SenderUsername == _signalRService.LoggedInUsername;
            var alignment = isMyMessage ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            var backgroundColor = isMyMessage ? (SolidColorBrush)new BrushConverter().ConvertFrom("#FFE8F5E8") : (SolidColorBrush)new BrushConverter().ConvertFrom("#FFF0F8FF");
            var senderText = isMyMessage ? "You say:" : $"{message.SenderUsername} says:";

            // Replace emoji codes
            string processedContent = message.Content;
            foreach (var emoji in EmojiMap)
            {
                processedContent = processedContent.Replace(emoji.Key, emoji.Value);
            }

            var messageBorder = new Border
            {
                Background = backgroundColor,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(8),
                HorizontalAlignment = alignment,
                MaxWidth = 300
            };

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock { Text = senderText, FontSize = 9, Foreground = Brushes.Gray, FontFamily = new FontFamily("Tahoma"), Margin = new Thickness(0, 0, 0, 2) });
            stackPanel.Children.Add(new TextBlock { Text = processedContent, FontSize = 11, FontFamily = new FontFamily("Tahoma"), TextWrapping = TextWrapping.Wrap });
            stackPanel.Children.Add(new TextBlock { Text = message.Timestamp.ToLocalTime().ToString("t"), FontSize = 8, Foreground = Brushes.DarkGray, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 2, 0, 0) });

            messageBorder.Child = stackPanel;
            MessagesPanel.Children.Add(messageBorder);
            ChatScrollViewer.ScrollToEnd();
        }
    }
}