using System.Collections.Generic;
using System.Linq;
using VideoOS.Platform;

namespace ServerInfo.Services
{
    public class MyConfigurationService
    {
        //Props

        //Fields

        //Constructors        

        //Methods
        public List<Item> GetDefinedItems(List<Item> folders)
        {
            var configItems = Configuration.Instance.GetItems(ItemHierarchy.SystemDefined);
            if (configItems != null) 
            {
                if (folders.Any())
                {
                    folders = new List<Item>();
                }                

                foreach (Item item in configItems)
                {
                    folders.Add(item);
                }                

                return folders;
            }
            
            return null;
        }
    }
}
