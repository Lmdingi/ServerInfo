using System;
using ServerInfo.Services;
using System.Windows;
using VideoOS.Platform;
using VideoOS.Platform.UI.Controls;
using VideoOS.Platform.UI;
using System.Collections.Generic;

namespace ServerInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : VideoOSWindow
    {
        //Props

        //Fields        
        private List<Item> _folders = new List<Item>();
        MyConfigurationService _myConfigurationService;

        //Constructors 
        public MainWindow()
        {
            InitializeComponent();
            _myConfigurationService = new MyConfigurationService();
        }

        //Methods
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            _itemPicker.Items = new List<Item>();
            _itemPicker.Items = _myConfigurationService.GetDefinedItems(_folders);
        }

        private void ItemPickerOnSelectedItemChanged(object sender, EventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }
        
    }
}
