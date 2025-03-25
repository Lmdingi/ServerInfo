using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using VideoOS.Platform;
using VideoOS.Platform.Messaging;
using VideoOS.Platform.UI;

namespace ServerInfo.Services
{
    public class MyConfigurationService
    {
        //Props
        //public List<Item> Items { get; set; }

        //Fields        
        //private List<Item> _folders = new List<Item>();
        private MessageCommunication _messageCommunication;
        private object _systemConfigurationChangedIndicationReference;
        private System.Threading.Timer _catchUpTimer;
        ItemPickerWpfUserControl _itemPickerWpfUserControl;

        //private List<Item> _filledItems = new List<Item>();
        private List<Item> _folders = new List<Item>();
        private Item _selectedItem = null;
        private List<FQID> _selectedDevices = null;

        //Constructors        
        public MyConfigurationService()
        {
            //Items = Configuration.Instance.GetItems();
        }

        //Methods
        public ItemPickerWpfUserControl FillContent(ItemPickerWpfUserControl itemPicker)
        {
            var configItems = Configuration.Instance.GetItems(ItemHierarchy.Both);
            if (configItems != null) 
            {
                if (_folders.Any())
                {
                    _folders = new List<Item>();
                }

                itemPicker.Items = new List<Item>();

                foreach (Item item in configItems)
                {
                    _folders.Add(item);
                }

                if (EnvironmentManager.Instance.MasterSite != null)
                {
                    Item masterSite = EnvironmentManager.Instance.GetSiteItem(EnvironmentManager.Instance.MasterSite);

                    if (masterSite != null)
                    {
                        ServerId serverId = new ServerId(ServerId.CorporateManagementServerType, "top", 1, Guid.NewGuid());

                        Item folder = new Item(serverId, Guid.Empty, Guid.NewGuid(), "Site-Hierarchy", FolderType.No, Kind.Folder);

                        var masterKids = configItems[1].GetChildren();
                        List<Item> items = new List<Item>();

                        foreach (var kid in masterKids)
                        {
                            items.Add(kid);
                        }

                        items.Add(masterSite);

                        folder.SetChildren(items);
                        _folders.Add(folder);
                    }
                }

                itemPicker.Items = _folders;

                return itemPicker;
            }
            
            return null;
        }

        public void AppAndSytemCommunicationManager()
        {
            MessageCommunicationManager.Start(EnvironmentManager.Instance.MasterSite.ServerId);

            _messageCommunication = MessageCommunicationManager.Get(EnvironmentManager.Instance.MasterSite.ServerId);

            _systemConfigurationChangedIndicationReference = _messageCommunication.RegisterCommunicationFilter((VideoOS.Platform.Messaging.Message message, FQID dest, FQID source) =>
            {
                List<FQID> fqids = message.Data as List<FQID>;
                if (fqids == null)
                {
                    // Start timer, when the detailed info is not available.  
                    _catchUpTimer.Change(TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(90));
                    return null;
                }

                // Detailed info received - stop catchup timer
                _catchUpTimer.Change(Timeout.Infinite, Timeout.Infinite);     // Disable timer, as we now have the detailed changes

                HashSet<FQID> serverFQIDList = new HashSet<FQID>();
                foreach (FQID fqid in fqids)
                {
                    Item item = Configuration.Instance.GetItem(fqid);
                    if (item != null)
                    {
                        Trace.WriteLine("SystemConfigurationChangedIndication - received -- for: " + item.Name);
                        FQID recorderFQID;
                        if (fqid.Kind == Kind.Server)
                            recorderFQID = fqid;
                        else
                            recorderFQID = fqid.GetParent();
                        if (recorderFQID != null)
                            serverFQIDList.Add(recorderFQID);
                    }
                    else
                    {
                        Trace.WriteLine("SystemConfigurationChangedIndication - received -- for: Unknown Item");
                        // unknown item - we will reload entire configuration
                        serverFQIDList.Clear();
                        serverFQIDList.Add(Configuration.Instance.ServerFQID);
                        break;
                    }
                }

                Thread reloadThread = new Thread(new ParameterizedThreadStart((object obj) =>
                {
                    HashSet<FQID> newServerFQIDList = obj as HashSet<FQID>;
                    if (newServerFQIDList != null)
                    {
                        // Now ask SDK to reload configuration from server, this will issue the "LocalConfigurationChangedIndication"
                        foreach (FQID serverFQID in newServerFQIDList)
                            VideoOS.Platform.SDK.Environment.ReloadConfiguration(serverFQID);
                    }
                }));

                reloadThread.Start(serverFQIDList);

                return null;
            },
                    new CommunicationIdFilter(MessageId.System.SystemConfigurationChangedIndication));
        }
    }
}
