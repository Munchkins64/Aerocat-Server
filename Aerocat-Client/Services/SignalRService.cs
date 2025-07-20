// In project: Aerocat.Client
// File: Services/SignalRService.cs
using Aerocat.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aerocat.Client.Services
{
    public class SignalRService
    {
        private readonly HubConnection _connection;
        public string LoggedInUsername { get; private set; }

        public event Action<Message> OnMessageReceived;
        public event Action<UserViewModel> OnUserStatusChanged;
        public event Action<string> OnUserRegistered;

        public SignalRService()
        {
            // IMPORTANT: The URL must match your server's URL.
            // When running from Visual Studio, find the HTTPS URL in the launchSettings.json of the server project.
            // It might be something like "https://localhost:7123"
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5150/chathub") // <-- CHANGE THIS URL
                .WithAutomaticReconnect()
                .Build();

            // Register handlers for server-to-client calls
            _connection.On<Message>("ReceiveMessage", (message) => OnMessageReceived?.Invoke(message));
            _connection.On<UserViewModel>("UserStatusChanged", (user) => OnUserStatusChanged?.Invoke(user));
            _connection.On<string>("UserRegistered", (username) => OnUserRegistered?.Invoke(username));
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                // Handle connection error
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            return await _connection.InvokeAsync<bool>("Register", username, password);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var success = await _connection.InvokeAsync<bool>("Login", username, password);
            if (success)
            {
                LoggedInUsername = username;
            }
            return success;
        }

        public async Task<List<UserViewModel>> GetOnlineUsersAsync()
        {
            return await _connection.InvokeAsync<List<UserViewModel>>("GetOnlineUsers");
        }

        public async Task SendMessageAsync(string receiverUsername, string content)
        {
            await _connection.InvokeAsync("SendMessage", receiverUsername, content);
        }

        public async Task<List<string>> SearchUsersAsync(string query)
        {
            return await _connection.InvokeAsync<List<string>>("SearchUsers", query);
        }
    }
}