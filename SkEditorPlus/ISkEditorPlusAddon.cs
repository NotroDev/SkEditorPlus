
namespace SkEditorPlus
{
    public interface ISkEditorPlusAddon
    {
        public string Name { get; }
        public string Author { get; }
        public string Description { get; }
        public string Version { get; }

        public int ApiVersion { get; }


        public void OnEnable(SkEditorAPI skEditor);

        void OnLoadFinished()
        {
            return;
        }

        void OnAllAddonsLoaded()
        {
            return;
        }

        void OnTabChanged()
        {
            return;
        }

        void OnTabCreate()
        {
            return;
        }

        void OnTabClose()
        {
            return;
        }

        void OnUnhandledException()
        {
            return;
        }

        void OnFileOpened(string fileName)
        {
            return;
        }

        void OnFileSave(string fileName)
        {
            return;
        }

        void OnFileCreate(string fileName)
        {
            return;
        }

        void OnPublish(string url)
        {
            return;
        }

        void OnQuickEdit(QuickEditType type)
        {
            return;
        }
        enum QuickEditType
        {
            CHANGE_DOTS_TO_COLONS, CHANGE_SPACES_TO_TABS, CHANGE_TABS_TO_SPACES, REMOVE_COMMENTS, SHORTEN_ELSE_IF
        }

        void OnGenerate(GenerateType type)
        {
            return;
        }

        enum GenerateType
        {
            GUI, COMMAND
        }

        void OnBackPackAdd(string addedString)
        {
            return;
        }

        void OnBackPackRemove(string removedString)
        {
            return;
        }

        void OnBackPackPaste(string pastedString)
        {
            return;
        }

        void OnCompletionAccept(string completion)
        {
            return;
        }

        void OnAddonInstall(string addonName, string author, string version)
        {
            return;
        }

        void OnAddonUninstall(string addonName, string author, string version)
        {
            return;
        }

        void OnAddonUpdate(string addonName, string author, string version)
        {
            return;
        }

        void OnSyntaxDisable()
        {
            return;
        }

        void OnSyntaxChange(string newSyntax)
        {
            return;
        }

        void OnSiteOpen(string name, string url)
        {
            return;
        }

        void OnCheckForUpdates(string currentVersion, string newVersion)
        {
            return;
        }

        void OnExiting()
        {
            return;
        }

    }
}