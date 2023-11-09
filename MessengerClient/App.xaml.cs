using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessengerClient.Core.Models;
using MessengerClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace MessengerClient
{
    public partial class App : Application
    {
        public static App Instance { get; private set; }

        //private readonly IHost _host;

        private AppClient _appClient;

        private INotifyPropertyChanged _currentViewModel;
        
        private AuthorizationViewModel _authorizationViewModel;
        private ChatViewModel _chatViewModel;

        public User CurrentUser { get; private set; }

        public App()
        {
            Instance = this;
            
            /*_host = Host.CreateDefaultBuilder()
                .ConfigureServices((services) =>
                {
                    services.AddSingleton<LoginWindow>();
                })
                .Build();*/
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            _appClient = new AppClient();
            _authorizationViewModel = new AuthorizationViewModel(_appClient);
            _chatViewModel = new ChatViewModel(_appClient);

            _authorizationViewModel.ShowSignInWindow();
            _authorizationViewModel.OnSignedIn += OnAuthorize;
            _authorizationViewModel.OnSignedUp += OnAuthorize;
            
            _appClient.TryStartAsync();
            
            base.OnStartup(e);
        }

        private void OnAuthorize(User authorizedUser)
        {
            CurrentUser = authorizedUser;
            _authorizationViewModel.HideAllWindows();
            
            _chatViewModel.ShowWindow();
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            /*_host.StopAsync();
            _host.Dispose();*/

            _appClient.QuitAsync(() =>
            {
                _appClient.Dispose();
            });
            
            base.OnExit(e);
        }


        
        
        
    }
}