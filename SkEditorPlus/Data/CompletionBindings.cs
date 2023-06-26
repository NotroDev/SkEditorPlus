using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
