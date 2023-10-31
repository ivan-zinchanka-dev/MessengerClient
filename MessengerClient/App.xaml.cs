using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        
        private AuthorizationViewModel _authorizationViewModel;
        private ChatViewModel _chatViewModel;
        
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
            _chatViewModel = new ChatViewModel();
            
            
            _authorizationViewModel.Window.Show();
            _authorizationViewModel.Window.OnHidden += Shutdown;
            
            _authorizationViewModel.OnSignedIn += () =>
            {
                _authorizationViewModel.Window.OnHidden -= Shutdown;
                _authorizationViewModel.Window.Close();
                
                _chatViewModel.Window.Show();
                _chatViewModel.Window.OnHidden += Shutdown;
            };

            //_appClient.TryStartAsync();
            
            
            base.OnStartup(e);
        }
        

        protected override void OnExit(ExitEventArgs e)
        {
            /*_host.StopAsync();
            _host.Dispose();*/

            _appClient.Dispose();
            
            base.OnExit(e);
        }


        
        
        
    }
}