﻿using HandyControl.Controls;
using SkEditorPlus.Functionalities;
using SkEditorPlus.Utilities.Vaults;
using System;
using System.Collections.Specialized;
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
            var addonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "Addons");

            Directory.CreateDirectory(addonFolder);

            foreach (var dllFile in Directory.EnumerateFiles(addonFolder, "*.dll", SearchOption.AllDirectories))
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
