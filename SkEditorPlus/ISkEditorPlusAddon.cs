namespace SkEditorPlus
{
    public interface ISkEditorPlusAddon
    {
        public string Name { get; }
        public string Author { get; }
        public string Description { get; }
        public string Version { get; }


        public void OnEnable(SkEditorAPI skEditor);
    }
}