﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using MessengerCoreLibrary.Infrastructure;
using MessengerCoreLibrary.Models;
using MessengerCoreLibrary.Services.FileLogging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MessengerClientService
{
    public partial class MessengerService : ServiceBase
    {
        private const string AppFolderName = "MessengerClientService";
        private const string LogsFileName = "app_service.logs";
        
        private static readonly IEqualityComparer<Message> MessageEqualityComparer = new MessageEqualityComparer();
        private static readonly TimeSpan UpdatePeriod = TimeSpan.FromSeconds(1.0f);
        
        private readonly IPEndPoint _remoteEndPoint;
        private readonly ILogger _logger;
        
        private UdpClient _udpClient;
        private Timer _updateTimer;
        private IEnumerable<Message> _cachedMessages;
        
        private static string AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        
        public MessengerService(IPEndPoint remoteEndPoint)
        {
            _remoteEndPoint = remoteEndPoint;
            
            string pathToLogs = Path.Combine(AppDataPath, AppFolderName, LogsFileName);
            
            _logger = new FileLogger(pathToLogs, nameof(MessengerService));
            
            _logger.LogInformation("Trying to construct");
            
            InitializeComponent();

            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
            
            _logger.LogInformation("Constructed");
        }
        
        protected override void OnStart(string[] args)
        {
            _logger.LogInformation("Trying to start");
            
            _udpClient = new UdpClient();
            _updateTimer = new Timer(Update, null, UpdatePeriod, UpdatePeriod);
            
            _logger.LogInformation("Service started");
        }
        
        protected override void OnPause()
        {
            _updateTimer?.Dispose();
            _logger.LogInformation("Service paused");
        }

        protected override void OnContinue()
        {
            _logger.LogInformation("Trying to continue");
            
            _updateTimer = new Timer(Update, null, UpdatePeriod, UpdatePeriod);
            
            _logger.LogInformation("Service continued");
        }

        protected override void OnStop()
        {
            _updateTimer?.Dispose();
            _udpClient?.Close();
            
            _logger.LogInformation("Service stopped");
        }
        
        private void Update(object parameter)
        {
            ReceiveMessageListAsync(NotifyIfNeed);
        }
        
        private async void ReceiveMessageListAsync(Action<IEnumerable<Message>> onCompleteCallback)
        {
            Query query = new Query(QueryHeader.UpdateChat);
            byte[] binaryQuery = Encoding.UTF8.GetBytes(query.ToString());

            await _udpClient.SendAsync(binaryQuery, binaryQuery.Length, _remoteEndPoint);
            
            UdpReceiveResult rawResult = await _udpClient.ReceiveAsync();
            Response response = Response.FromRawLine(Encoding.UTF8.GetString(rawResult.Buffer));
            
            IEnumerable<Message> messageList = JsonConvert.DeserializeObject<IEnumerable<Message>>(response.JsonDataString);
            onCompleteCallback?.Invoke(messageList);
        }
        
        private void NotifyIfNeed(IEnumerable<Message> actualMessages)
        {
            if (_cachedMessages == null)
            {
                _cachedMessages = actualMessages;
            }
            else
            {
                IEnumerable<Message> newMessages = actualMessages.Except(_cachedMessages, MessageEqualityComparer);
                
                foreach (Message message in newMessages)
                {
                    _logger.LogInformation("\n{0}\n{1}\n{2}\n", 
                        message.SenderNickname, message.Text, message.PostDateTime);
                }

                _cachedMessages = actualMessages;
            }
        }
    }
}
