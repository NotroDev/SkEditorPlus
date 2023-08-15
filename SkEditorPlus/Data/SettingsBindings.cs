using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditorPlus.Data
{
    public partial class SettingsBindings : ObservableObject
    {
        [ObservableProperty]
        private bool isAutoSaveEnabled;

        [ObservableProperty]
        private bool isCheckForUpdatesEnabled;

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
        
        [ObservableProperty]
        private bool isProjectsExperimentEnabled;

        [ObservableProperty]
        private bool isCompletionExperimentEnabled;

        [ObservableProperty]
        private bool isBottomBarExperimentEnabled;

        [ObservableProperty]
        private bool isAnalyzerExperimentEnabled;
    }
}
