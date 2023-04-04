using HandyControl.Controls;
using SkEditorPlus.Functionalities;
using SkEditorPlus.Managers;
using System;
using System.Diagnostics;
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
                    AddonManager.addons.Add(addonInstance);
                }
                AddonManager.addons.ForEach(addon =>
                {
                    addon.OnAllAddonsLoaded();
                });
            }
            catch (Exception ex)
            {
                var addonInstance = ex.TargetSite.DeclaringType;
                if (addonInstance != null && typeof(ISkEditorPlusAddon).IsAssignableFrom(addonInstance))
                {
                    var addon = (ISkEditorPlusAddon)Activator.CreateInstance(addonInstance);
                    if (!addon.ApiVersion.Equals(skEditorAPI.GetApiVersion()))
                    {
                        Growl.Warning($"Addon \"{addon.Name}\" was not loaded, probably because it was created for a different API version.\n\nCreated for: {addon.ApiVersion}\nYour version: {skEditorAPI.GetApiVersion()}");
                    }
                }
                else
                {
                    MessageBox.Show("Error has been occurred while enabling one of addons.\n\nError message:\n" + ex.Message, "SkEditor+", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }

        }
    }
}
