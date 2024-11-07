using System;
using System.ServiceProcess;

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
