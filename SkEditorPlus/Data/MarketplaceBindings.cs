using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Renci.SshNet.Messages;
using SkEditorPlus.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SkEditorPlus.Data
{
    public partial class MarketplaceBindings : ObservableObject, INotifyPropertyChanged
    {
        [ObservableProperty]
        private ObservableCollection<MarketplaceItem> marketItems;

        [ObservableProperty]
        private ObservableCollection<MarketplaceItem> filteredItems;

        [ObservableProperty]
        private MarketplaceItem selectedItem;

        [ObservableProperty]
        private string installButtonHeader = "Install";

        public ICommand InstallCommand { get; }

        public MarketplaceBindings()
        {
            InstallCommand = new RelayCommand(InstallCommandExecute, InstallCommandCanExecute);
        }

        private bool InstallCommandCanExecute()
        {
            return true;
        }

        private void InstallCommandExecute()
        {
            string action = InstallButtonHeader.ToLower();

            switch (action)
            {
                case "install":
                    MarketplaceWindow.GetInstance().InstallAddon();
                    break;
                case "uninstall":
                    MarketplaceWindow.GetInstance().UninstallAddon();
                    break;
                case "update":
                    MarketplaceWindow.GetInstance().UpdateAddon();
                    break;
            }
        }
    }

    public class MarketplaceItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Icon { get; set; }
        public string LongDescription { get; set; }

        public string Type { get; set; }

        public string URL { get; set; }

        public string NamePlusVersion { get; set; }
}
}
