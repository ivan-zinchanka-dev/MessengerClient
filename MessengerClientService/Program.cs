using System;
using System.IO;
using System.Net;
using System.ServiceProcess;
using MessengerCoreLibrary.Services;

namespace MessengerClientService
{
    public static class Program
    {
        private static void Main()
        {
            if (Environment.UserInteractive)
            {
                new MessengerService();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new MessengerService() });
            }
        }
    }
}
