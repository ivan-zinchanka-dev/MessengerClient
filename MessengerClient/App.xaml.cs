using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MessengerClient.Core.Models;
using MessengerClient.Core.Services;
using MessengerClient.Core.Services.FileLogging;
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
        private readonly IniService _iniService;
        private readonly ILogger<App> _logger;
        
        private AppClient _appClient;
        private AuthorizationViewModel _authorizationViewModel;
        private ChatViewModel _chatViewModel;
        
        public User CurrentUser { get; private set; }
        public bool IsClientConnected => _appClient.IsConnected;
        public ChatUpdater ChatUpdater => _appClient.ChatUpdater;
        
        // TODO Add dll for Core
        
        public App()
        {
            string appConfigPath = Path.Combine(Directory.GetCurrentDirectory(), AppConfigFileName);
            _iniService = new IniService(appConfigPath);
            
            _sharedOptions = new AppSharedOptions(GetRemoteEndPoint());
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder
                        .AddConsole()
                        .AddFile(GetLogsFileName())
                        .SetMinimumLevel(LogLevel.Information);
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

            _logger = _host.Services.GetRequiredService<ILogger<App>>();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogError( e.Exception, "Unhandled exception occured");
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception occured"); 
            }
            else
            {
                _logger.LogError(e.ExceptionObject.ToString());
            }
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
            base.OnStartup(e);
            
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            await _host.StartAsync();
            _appClient = _sharedOptions.AppClient;
            
            _authorizationViewModel = _host.Services
                .GetRequiredService<AuthorizationViewModel>();
            
            _authorizationViewModel.ShowSignInWindow();
        }
        
        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            
            AppDomain.CurrentDomain.UnhandledException -= OnAppDomainUnhandledException;
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
            
            base.OnExit(e);
        }
        
        private void Authorize(User authorizedUser)
        {
            CurrentUser = authorizedUser;
            _authorizationViewModel.HideAllWindows();
            
            _chatViewModel = _host.Services.GetRequiredService<ChatViewModel>();
            _chatViewModel.ShowWindow();
        }

        private string GetLogsFileName()
        {
            return _iniService.GetString("Logging", "LogsFileName");
        }

        private IPEndPoint GetRemoteEndPoint()
        {
            string addressString = _iniService.GetString(
                nameof(_sharedOptions.RemoteEndPoint),
                nameof(_sharedOptions.RemoteEndPoint.Address));
            
            string portString = _iniService.GetString(
                nameof(_sharedOptions.RemoteEndPoint),
                nameof(_sharedOptions.RemoteEndPoint.Port));
            
            return new IPEndPoint(IPAddress.Parse(addressString), Convert.ToInt32(portString));
        }
    }
}