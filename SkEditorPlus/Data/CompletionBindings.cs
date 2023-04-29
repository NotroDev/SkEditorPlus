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
    public partial class CompletionBindings : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> completionItems;

        private IEnumerable<CompletionDataElement> _completionDataElements;

        public IEnumerable<CompletionDataElement> CompletionDataElements
        {
            get => _completionDataElements;
            set
            {
                _completionDataElements = value;
                CompletionItems = new ObservableCollection<string>(value.Select(item => item.Name));
            }
        }
    }
}
