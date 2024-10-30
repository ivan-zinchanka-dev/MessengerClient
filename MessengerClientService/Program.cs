using System;
using System.IO;
using System.Net;
using System.ServiceProcess;
using MessengerCoreLibrary.Services;

namespace MessengerClientService
{
    public static class Program
    {
        private const string ServiceConfigFileName = "service_config.ini";
        private static readonly IniService IniService = new IniService(
            Path.Combine(Directory.GetCurrentDirectory(), ServiceConfigFileName));
        
        private static void Main()
        {
            if (Environment.UserInteractive)
            {
                new MessengerService(GetRemoteEndPoint());
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new MessengerService(GetRemoteEndPoint()) });
            }
        }
        
        private static IPEndPoint GetRemoteEndPoint()
        {
            string addressString = IniService.GetString("RemoteEndPoint", "Address");
            string portString = IniService.GetString("RemoteEndPoint", "Port");
            
            return new IPEndPoint(IPAddress.Parse(addressString), Convert.ToInt32(portString));
        }
    }
}
