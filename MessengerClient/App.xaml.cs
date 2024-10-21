using System.ComponentModel;
using System.Windows;
using MessengerClient.Core.Models;
using MessengerClient.Network;
using MessengerClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace MessengerClient
{
    public partial class App : Application
    {
        public static App Instance { get; private set; }

        private readonly IHost _host;
        
        private AppClient _appClient;
        private AuthorizationViewModel _authorizationViewModel;
        private ChatViewModel _chatViewModel;

        public User CurrentUser { get; private set; }

        public App()
        {
            Instance = this;
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .AddSingleton<AppClient>()
                        .AddSingleton<AuthorizationViewModel>()
                        .AddSingleton<ChatViewModel>();
                    
                })
                .Build();
        }
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync().ConfigureAwait(false);

            _appClient = _host.Services.GetRequiredService<AppClient>();

            bool isConnected = await _appClient.TryConnectAsync();

            _authorizationViewModel = _host.Services
                .GetRequiredService<AuthorizationViewModel>();
            
            _authorizationViewModel.ShowSignInWindow();
            _authorizationViewModel.OnSignedIn += OnAuthorize;
            _authorizationViewModel.OnSignedUp += OnAuthorize;
            
            base.OnStartup(e);
        }

        private void OnAuthorize(User authorizedUser)
        {
            CurrentUser = authorizedUser;
            _authorizationViewModel.HideAllWindows();
            
            _chatViewModel = _host.Services.GetRequiredService<ChatViewModel>();
            _chatViewModel.ShowWindow();
        }
        
        protected override async void OnExit(ExitEventArgs e)
        {
            await _appClient.QuitAsync();
            _appClient.Dispose();
            
            await _host.StopAsync().ConfigureAwait(false);
            _host.Dispose();
            
            base.OnExit(e);
        }
    }
}