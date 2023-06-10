using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;

namespace SkEditorPlus.Utilities
{
    public class CodeFromWeb
    {
        public static async Task<string> GetCodeFromSkript(string id)
        {
            string url = $"https://code.skript.pl/{id}/raw";

            using HttpClient client = new();
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (HttpRequestException)
            {
                HandyControl.Controls.MessageBox.Error(Application.Current.FindResource("FailedToSendRequestError") as string);
                return null;
            }
            string content = await response.Content.ReadAsStringAsync();

            if (response.Content.Headers.ContentType.MediaType != "text/plain")
            {
                HandyControl.Controls.MessageBox.Error((Application.Current.FindResource("ScriptWithIDNotFoundError") as string).Replace("{0}", id));
                return null;
            }

            string path = Path.Combine(Path.GetTempPath(), $"{id}.sk");
            File.WriteAllText(path, content);
            return path;
        }
    }
}
