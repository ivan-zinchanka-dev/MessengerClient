using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerClientService
{
    [RunInstaller(true)]
    public partial class MessengerServiceInstaller : System.Configuration.Install.Installer
    {
        public MessengerServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
