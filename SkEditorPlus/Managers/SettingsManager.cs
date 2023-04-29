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

namespace SkEditorPlus.Managers
{
    public class SettingsManager
    {

        public static void SaveSettings()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor Plus", "settings.json");

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
            string settingsFolder = "SkEditor Plus";
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsFile = "settings.json";

            string path = Path.Combine(appDataFolder, settingsFolder, settingsFile);

            if (!File.Exists(path)) return;

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
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "SkEditor Plus", "Syntax Highlighting",
                            installedSyntax.Split('|')[1])))
                    .ToList();

                itemsToRemove.ForEach(item => Properties.Settings.Default.InstalledSyntaxes.Remove(item));

                Properties.Settings.Default.Save();
            }
        }
    }
}
