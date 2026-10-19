using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PDF_downloader
{
    public class settings_handler
    {
        Dictionary<string, string> text_values = new();
        string filename = "\\settings.txt";
        public void startup()
        {
            if (!File.Exists(Environment.CurrentDirectory + filename)) return;
            foreach (string line in File.ReadAllLines(Environment.CurrentDirectory + filename))
            { 
                text_values.Add(line.Split("¤").First(), line.Split("¤").Last());
            }
        }

        public void save_setting(List<string> settings)
        {
            File.WriteAllLines(Environment.CurrentDirectory + filename, settings);
        }

        public string get_value (string key)
        {
                return text_values[key];
        }
    }
}
