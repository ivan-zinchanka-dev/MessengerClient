using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using MessengerClient.Management;
using MessengerClient.Network;
using MessengerClient.ViewModels;
using MessengerClient.Views;
using MessengerCoreLibrary.Services;
using MessengerCoreLibrary.Services.FileLogging;
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

        private BindingErrorTraceListener _bindingTraceListener;
        private WindowManager _windowManager;

        public IServiceProvider Services => _host.Services;
        
        public App()
        {
            string appConfigPath = Path.Combine(Directory.GetCurrentDirectory(), AppConfigFileName);
            _iniService = new IniService(appConfigPath);
            
            _sharedOptions = new AppSharedOptions(GetRemoteEndPoint());
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder
                        .AddDebug()
                        .AddFile(GetLogsFileName())
                        .SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<AppSharedOptions>(_sharedOptions)
                        .AddSingleton<App>(this)
                        .AddHostedService<AppClient>()
                        .AddSingleton<WindowManager>()
                        .AddTransient<AuthorizationViewModel>()
                        .AddTransient<ChatViewModel>()
                        .AddTransient<SignInWindow>()
                        .AddTransient<SignUpWindow>()
                        .AddTransient<ChatWindow>();
                })
                .Build();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();
        }
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _bindingTraceListener = new BindingErrorTraceListener();
            PresentationTraceSources.DataBindingSource.Listeners.Add(_bindingTraceListener);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            await _host.StartAsync();
            
            _windowManager = _host.Services.GetRequiredService<WindowManager>();
            _windowManager.SwitchTo<SignInWindow>();
            _windowManager.OnAllWindowsClosed += Shutdown;
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            
            AppDomain.CurrentDomain.UnhandledException -= OnAppDomainUnhandledException;
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
            
            PresentationTraceSources.DataBindingSource.Listeners.Remove(_bindingTraceListener);
            
            base.OnExit(e);
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
        
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            _logger.LogError(eventArgs.Exception, "An unhandled exception occurred.");
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            if (eventArgs.ExceptionObject is Exception exception)
            {
                _logger.LogError(exception, "An unhandled exception occurred."); 
            }
            else
            {
                _logger.LogError(eventArgs.ExceptionObject.ToString());
            }
        }
    }
}