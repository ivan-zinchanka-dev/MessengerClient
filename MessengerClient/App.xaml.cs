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
            /*_host.Start();

            MainWindow = _host.Services.GetRequiredService<LoginWindow>();
            MainWindow.Show();*/


            _authorizationViewModel = new AuthorizationViewModel();
            _chatViewModel = new ChatViewModel();
            
            
            _authorizationViewModel.Window.Show();
            
            _authorizationViewModel.OnSignedIn += () =>
            {
                _authorizationViewModel.Window.Close();
                _chatViewModel.Window.Show();
            };

            //MainWindow = new LoginWindow();
            //MainWindow.Show();
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            /*_host.StopAsync();
            _host.Dispose();*/

            base.OnExit(e);
        }


        
        
        
    }
}