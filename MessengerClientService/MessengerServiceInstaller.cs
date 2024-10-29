using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace MessengerClientService
{
    [RunInstaller(true)]
    public partial class MessengerServiceInstaller : Installer
    {
        private ServiceInstaller _serviceInstaller;
        private ServiceProcessInstaller _processInstaller;

        public MessengerServiceInstaller()
        {
            InitializeComponent();
            _serviceInstaller = new ServiceInstaller();
            _processInstaller = new ServiceProcessInstaller();

            _serviceInstaller.StartType = ServiceStartMode.Manual;
            _serviceInstaller.ServiceName = nameof(MessengerService);
            _processInstaller.Account = ServiceAccount.LocalSystem;

            Installers.Add(_processInstaller);
            Installers.Add(_serviceInstaller);
        }
    }
}
