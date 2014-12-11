using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WifiWebLogin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WatchAndLoginCoordinator _watchAndLoginCoordinator;

        protected override void OnStartup(StartupEventArgs e)
        {
            //load saved WifiLogins
            //hard coded for now
            var wifiLogins = new List<WifiLogin> {new WifiLogin
            {
                DetectionType = DetectionTypeEnum.DetectByRedirect,
                RedirectHost = "***",
                Username = "***",
                Password = "***",
            }}; 

            _watchAndLoginCoordinator = new WatchAndLoginCoordinator(wifiLogins);

            base.OnStartup(e);
        }
    }
}
