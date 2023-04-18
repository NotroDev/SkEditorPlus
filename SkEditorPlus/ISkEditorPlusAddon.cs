using HandyControl.Controls;

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

        void OnUnhandledException()
        {
            return;
        }
    }
}