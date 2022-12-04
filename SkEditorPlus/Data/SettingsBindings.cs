using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkEditorPlus.Data
{
    public partial class SettingsBindings : ObservableObject
    {
        [ObservableProperty]
        private bool isAutoSaveEnabled;

        [ObservableProperty]
        private bool isWrappingEnabled;

        [ObservableProperty]
        private bool isAutoSecondCharacterEnabled;
        
        [ObservableProperty]
        private bool isAutoNewLineAndTabEnabled;
        
        [ObservableProperty]
        private bool isMicaEnabled;
        
        [ObservableProperty]
        private bool isDiscordRPCEnabled;
    }
}
