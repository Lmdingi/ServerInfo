using System;
using ServerInfo.Services;
using System.Windows;
using VideoOS.Platform;
using VideoOS.Platform.UI.Controls;
using VideoOS.Platform.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace ServerInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : VideoOSWindow
    {
        //Props
        public List<Item> Folders { get; set; } = new List<Item>();

        //Fields  
        private readonly MyConfigurationService _myConfigurationService;

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
            _itemPicker.Items = _myConfigurationService.GetDefinedItems(Folders);
        }

        private void ItemPicker_SelectedItemChanged(object sender, EventArgs e)
        {

            if (_itemPicker.SelectedItems.Any())
            {
                var selectedItem = _itemPicker.SelectedItems.First();
                DisplaySelectedItemDetails(selectedItem);
            }
        }

        private void DisplaySelectedItemDetails(Item item)
        {
            if (item != null)
            {
                item = Configuration.Instance.GetItem(item.FQID) ?? item;

                var selectedItemRoot = new VideoOSTreeViewItem();
                selectedItemRoot.Data = item.Name;
                selectedItemRoot.IsExpanded = true;

                List<VideoOSTreeViewItem> children = new List<VideoOSTreeViewItem>();

                var members = item.GetType().GetProperties();

                foreach (PropertyInfo prop in members)
                {
                    var value = prop.GetValue(item);
                    var child = MakeAVideoOSTreeViewItem(prop.Name, value);

                    if(child != null && !string.IsNullOrEmpty((string)child.Data))
                    {
                        children.Add(child);
                    }
                }


                selectedItemRoot.Children = children;

                selectedItemDetails.ItemsSource = new List<VideoOSTreeViewItem>() { selectedItemRoot };
            }
        }

        private VideoOSTreeViewItem MakeAVideoOSTreeViewItem<T>(string itemMemberName, T value)
        {
            VideoOSTreeViewItem videoOSTreeViewItem = new VideoOSTreeViewItem();
            videoOSTreeViewItem.IsExpanded = true;

            if(value != null)
            {
                if (value.GetType() == typeof(string) || value.GetType().IsPrimitive)
                {
                    videoOSTreeViewItem.Data = $"{itemMemberName}: {value}";
                }
                else if (value is IDictionary<string, string> dictionary)
                {
                    videoOSTreeViewItem.Data = itemMemberName;
                    List<VideoOSTreeViewItem> children = new List<VideoOSTreeViewItem>();

                    foreach (var pair in dictionary)
                    {
                        children.Add(new VideoOSTreeViewItem() { Data = pair.Key + " = " + pair.Value });
                    }

                    videoOSTreeViewItem.Children = children;
                }
                else if (value.GetType().IsClass && value.GetType() != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(value.GetType()) && !typeof(IDictionary).IsAssignableFrom(value.GetType()))
                {
                    videoOSTreeViewItem.Data = itemMemberName;
                    List<VideoOSTreeViewItem> children = new List<VideoOSTreeViewItem>();

                    var members = value.GetType().GetProperties();

                    foreach (PropertyInfo prop in members)
                    {
                        var propValue = prop.GetValue(value);
                        children.Add(new VideoOSTreeViewItem() { Data = prop.Name + ": " + propValue });
                    }

                    videoOSTreeViewItem.Children = children;
                }
                else if (value is Enum)
                {
                    videoOSTreeViewItem.Data = $"{itemMemberName}: {value}";
                }
                else if (value is Guid)
                {
                    videoOSTreeViewItem.Data = $"{itemMemberName}: {value}";
                }

                    return videoOSTreeViewItem;
            }

            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }
        
    }
}
