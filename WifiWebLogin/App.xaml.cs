using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;

namespace WifiWebLogin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WatchAndLoginCoordinator _watchAndLoginCoordinator;
        private static readonly string _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WifiWebLoginData");
        private readonly string _configFilePath = Path.Combine(_dataDirectory, "config.xml");
        private WifiLoginConfig _config;

        protected override void OnStartup(StartupEventArgs e)
        {
            _config = ReadConfig();
            
            _watchAndLoginCoordinator = new WatchAndLoginCoordinator(_config);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _watchAndLoginCoordinator.Stop();
            StoreConfig(_config);
            base.OnExit(e);
        }

        private WifiLoginConfig ReadConfig()
        {
            var seri = new DataContractSerializer(typeof (WifiLoginConfig));

            var config = new WifiLoginConfig();
            if (File.Exists(_configFilePath) && new FileInfo(_configFilePath).Length > 0)
            {
                using (var stream = File.Open(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    config = seri.ReadObject(stream) as WifiLoginConfig;
                    return config;
                }
            }
            return config;
        }

        private void StoreConfig(WifiLoginConfig config)
        {
            var seri = new DataContractSerializer(typeof(WifiLoginConfig));

            File.Delete(_configFilePath);
            using (var stream = File.Open(_configFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                seri.WriteObject(stream,config);
            }
            
        }

    }
}
