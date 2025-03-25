using System;
using ServerInfo.Services;
using System.Windows;
using VideoOS.Platform;
using VideoOS.Platform.UI.Controls;
using VideoOS.Platform.UI;

namespace ServerInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : VideoOSWindow
    {
        //private ConfigManager _configManager = new ConfigManager();
        MyConfigurationService _myConfigurationService;
        //ItemPickerWpfUserControl _itemPicker;
        public MainWindow()
        {
            InitializeComponent();
            _myConfigurationService = new MyConfigurationService();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            _itemPicker =  _myConfigurationService.FillContent(_itemPicker);
            //_myConfigurationService.AppAndSytemCommunicationManager();

            EnvironmentManager.Instance.EnvironmentOptions[EnvironmentOptions.ConfigurationChangeCheckInterval] = "10";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }

        private void ItemPickerOnSelectedItemChanged(object sender, EventArgs e)
        {

        }
    }
}
