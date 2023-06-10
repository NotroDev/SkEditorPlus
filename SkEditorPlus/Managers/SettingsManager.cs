using HandyControl.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Managers
{
    public class SettingsManager
    {
        private static readonly string settingsFolder = "SkEditor Plus";
        private static readonly string settingsFile = "settings.json";
        private static readonly string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string path = Path.Combine(appDataFolder, settingsFolder, settingsFile);
        private static readonly string backupFolder = "Backup";
        private static readonly string backupPath = Path.Combine(appDataFolder, settingsFolder, backupFolder, settingsFile);

        public static void SaveSettings()
        {
            CreateBackup();

            var settings = new JObject();
            var sortedProperties = Properties.Settings.Default.Properties.Cast<SettingsProperty>()
                .OrderBy(p => p.Name);

            foreach (SettingsProperty property in sortedProperties)
            {
                if (Properties.Settings.Default[property.Name] is not null)
                {
                    settings.Add(new JProperty(property.Name, JToken.FromObject(Properties.Settings.Default[property.Name])));
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, settings.ToString(Formatting.Indented));
        }

        public static void LoadSettings()
        {
            if (!File.Exists(path))
            {
                RestoreDefaultSettings();
                return;
            }

            try
            {
                var settings = JObject.Parse(File.ReadAllText(path));

                foreach (SettingsProperty property in Properties.Settings.Default.Properties)
                {
                    if (settings.TryGetValue(property.Name, out JToken value))
                    {
                        Properties.Settings.Default[property.Name] = value.ToObject(property.PropertyType);
                    }
                }

                Properties.Settings.Default.Save();

                if (Properties.Settings.Default.InstalledSyntaxes is not null)
                {
                    var itemsToRemove = Properties.Settings.Default.InstalledSyntaxes
                        .Cast<string>()
                        .Where(installedSyntax =>
                            !File.Exists(Path.Combine(
                                appDataFolder,
                                settingsFolder,
                                "Syntax Highlighting",
                                installedSyntax.Split('|')[1])))
                        .ToList();

                    itemsToRemove.ForEach(item => Properties.Settings.Default.InstalledSyntaxes.Remove(item));

                    Properties.Settings.Default.Save();
                }
            }
            catch (JsonReaderException)
            {
                RestoreFromBackup();
            }
        }

        private static void CreateBackup()
        {
            Directory.CreateDirectory(Path.Combine(appDataFolder, settingsFolder, backupFolder));
            File.Copy(path, backupPath, true);
        }

        private static void RestoreFromBackup()
        {
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, path, true);
                LoadSettings();
            }
            else
            {
                RestoreDefaultSettings();
            }
            MessageBox.Show("Your settings file was corrupted and has been restored to the last working version.", "Settings File Corrupted", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static void RestoreDefaultSettings()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            CreateDefaultSettingsFile();
            MessageBox.Show("Your settings file was corrupted and has been restored to the default settings.", "Settings File Corrupted", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static void CreateDefaultSettingsFile()
        {
            var defaultSettings = new JObject();

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, defaultSettings.ToString(Formatting.Indented));
        }
    }
}
