using SkEditorPlus.Functionalities;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Functionalities
{
    class AddonLoaderFunc : IFunctionality
    {
        public void OnEnable(SkEditorAPI skEditorAPI)
        {
            var appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Directory.CreateDirectory(Path.Combine(appdataFolder, "SkEditor Plus", "Addons"));
            directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            var addonFolder = Path.Combine(appdataFolder, "SkEditor Plus", "Addons");
            
            
            foreach (var dllFile in Directory.EnumerateFiles(addonFolder, "*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFrom(dllFile);
            }
            try
            {
                var addonTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeof(ISkEditorPlusAddon).IsAssignableFrom(p))
                    .Where(w => w.IsClass && !w.IsAbstract);
                foreach (var addonType in addonTypes)
                {
                    var addonInstance = (ISkEditorPlusAddon)Activator.CreateInstance(addonType);
                    addonInstance.OnEnable(skEditorAPI);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error has been occurred while enabling one of addons.\n\nError message:\n" + ex.Message, "SkEditor+", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
    }
}
