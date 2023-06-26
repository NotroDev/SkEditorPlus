using HandyControl.Controls;
using HandyControl.Tools.Extension;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Functionalities
{
    class AddonLoaderFunc : IFunctionality
    {
        public void OnEnable(SkEditorAPI skEditorAPI)
        {
            var addonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Addons");

            Directory.CreateDirectory(addonFolder);

            string[] experimentAddons = { "CompletionUnlocker", "Analyzer", "ProjectsAndStructureUnlocker" };

            IEnumerable<string> dllFiles = Directory.EnumerateFiles(addonFolder, "*.dll", SearchOption.AllDirectories);
            IEnumerable<string> experimentAddonsDlls = dllFiles.Where(dll => experimentAddons.Contains(Path.GetFileNameWithoutExtension(dll)));
            dllFiles = dllFiles.Except(experimentAddonsDlls);

            foreach (var dllFile in dllFiles)
            {
                StringCollection addonsToRemove = Properties.Settings.Default.AddonsToRemove;
                addonsToRemove ??= new StringCollection();


                if (addonsToRemove.Contains(Path.GetFileName(dllFile)))
                {
                    var updateFile = Path.Combine(Path.GetDirectoryName(dllFile), "update-" + Path.GetFileName(dllFile));
                    if (File.Exists(updateFile))
                    {
                        File.Delete(dllFile);
                        File.Move(updateFile, dllFile);
                        addonsToRemove.Remove(Path.GetFileName(dllFile));
                        Properties.Settings.Default.Save();
                        Assembly.LoadFrom(dllFile);
                        continue;
                    }

                    File.Delete(dllFile);
                    addonsToRemove.Remove(Path.GetFileName(dllFile));
                    Properties.Settings.Default.AddonsToRemove = addonsToRemove;
                    Properties.Settings.Default.Save();
                    continue;
                }

                Assembly.LoadFrom(dllFile.Replace("update-", ""));
            }

            if (experimentAddonsDlls.Any())
            {
                StringBuilder addonBuilder = new();
                experimentAddonsDlls.ForEach(a => addonBuilder.Append($"\"{Path.GetFileNameWithoutExtension(a)}\", "));
                addonBuilder.Remove(addonBuilder.Length - 2, 2);
                if (experimentAddonsDlls.Count() == 1)
                {
                    MessageBox.Warning($"You have installed the {addonBuilder} addon that was transformed into an Experiment now.\n\nThe addon will be removed - you can enable Experiment in the Settings.", "Experiments");
                }
                else
                {
                    MessageBox.Warning($"You have installed {experimentAddonsDlls.Count()} addons ({addonBuilder}) that were transformed into Experiments now.\n\nAddons will be removed - you can enable Experiments in the Settings.");
                }
                experimentAddonsDlls.ForEach(a => File.Delete(a));
            }

            try
            {
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeof(ISkEditorPlusAddon).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                    .Select(addonType => (ISkEditorPlusAddon)Activator.CreateInstance(addonType))
                    .ToList()
                    .ForEach(addon =>
                    {
                        addon.OnEnable(skEditorAPI);
                        AddonVault.addons.Add(addon);
                    });

                AddonVault.addons.ForEach(addon => addon.OnAllAddonsLoaded());
            }
            catch (Exception ex)
            {
                var addonInstance = ex.TargetSite?.DeclaringType;
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
                    MessageBox.Show($"Error has been occurred while enabling one of addons.\n\nError message:\n{ex.Message}", "SkEditor+", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }
        }
    }
}
