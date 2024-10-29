using System.ServiceProcess;

namespace MessengerClientService
{
    public static class Program
    {
        private static void Main()
        {
            ServiceBase.Run(new ServiceBase[] { new MessengerService() });
        }
    }
}
