using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using MessengerClient.Core.Models;
using MessengerClient.Core.Services;
using MessengerClient.Network;
using MessengerClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessengerClient
{
    public partial class App : Application
    {
        private const string AppConfigFileName = "app_config.ini";
        private readonly IHost _host;
        
        private readonly AppSharedOptions _sharedOptions;
        private AppClient _appClient;
        private AuthorizationViewModel _authorizationViewModel;
        private ChatViewModel _chatViewModel;

        public User CurrentUser { get; private set; }
        public bool IsClientConnected => _appClient.IsConnected;
        public ChatUpdater ChatUpdater => _appClient.ChatUpdater;
        
        public App()
        {
            _sharedOptions = new AppSharedOptions(GetRemoteEndPoint());
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<AppSharedOptions>(_sharedOptions)
                        .AddSingleton<App>(this)
                        .AddHostedService<AppClient>()
                        .AddSingleton<AuthorizationViewModel>()
                        .AddSingleton<ChatViewModel>();
                })
                .Build();
        }

        public async Task<bool> TrySignInAsync(User user)
        {
            bool result = await _appClient.TrySignInAsync(user);

            if (result)
            {
                Authorize(user);
            }

            return result;
        }
        
        public async Task<bool> TrySignUpAsync(User user)
        {
            bool result = await _appClient.TrySignUpAsync(user);
            
            if (result)
            {
                Authorize(user);
            }

            return result;
        }

        public async Task<bool> PostMessageAsync(Message message) => await _appClient.PostMessageAsync(message);
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            _appClient = _sharedOptions.AppClient;
            
            _authorizationViewModel = _host.Services
                .GetRequiredService<AuthorizationViewModel>();
            
            _authorizationViewModel.ShowSignInWindow();
            
            base.OnStartup(e);
        }
        
        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            
            base.OnExit(e);
        }
        
        private void Authorize(User authorizedUser)
        {
            CurrentUser = authorizedUser;
            _authorizationViewModel.HideAllWindows();
            
            _chatViewModel = _host.Services.GetRequiredService<ChatViewModel>();
            _chatViewModel.ShowWindow();
        } 
        
        private IPEndPoint GetRemoteEndPoint()
        {
            string appConfigPath = Path.Combine(Directory.GetCurrentDirectory(), AppConfigFileName);
            
            IniService iniService = new IniService(appConfigPath);

            string addressString = iniService.GetString(
                nameof(_sharedOptions.RemoteEndPoint),
                nameof(_sharedOptions.RemoteEndPoint.Address));
            
            string portString = iniService.GetString(
                nameof(_sharedOptions.RemoteEndPoint),
                nameof(_sharedOptions.RemoteEndPoint.Port));
            
            return new IPEndPoint(IPAddress.Parse(addressString), Convert.ToInt32(portString));
        }
    }
}