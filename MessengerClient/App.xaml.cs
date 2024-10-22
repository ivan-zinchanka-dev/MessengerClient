﻿using System;
using System.Threading.Tasks;
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
        public bool IsClientConnected => _appClient.IsConnected;
        public ChatUpdater ChatUpdater => _appClient.ChatUpdater;

        public void SetClient(AppClient appClient)      // TODO refactor (events?, shared service?)
        {
            _appClient = appClient;
            Console.WriteLine("Set client");
        }

        public App()
        {
            Instance = this;
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
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
        
        private void Authorize(User authorizedUser)
        {
            CurrentUser = authorizedUser;
            _authorizationViewModel.HideAllWindows();
            
            _chatViewModel = _host.Services.GetRequiredService<ChatViewModel>();
            _chatViewModel.ShowWindow();
        } 
            
        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            
            Console.WriteLine("After set client");
            
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
    }
}