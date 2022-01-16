using DaJet.Metadata.Model;
using System.IO;
using System.Text;

namespace DaJet.Metadata.Services
{
    public sealed class ConfigFileWriter
    {
        public void Write(ConfigObject config, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                WriteToFile(writer, config, 0, string.Empty);
            }
        }
        private void WriteToFile(StreamWriter writer, ConfigObject config, int level, string path)
        {
            string indent = level == 0 ? string.Empty : "-".PadLeft(level * 4, '-');

            for (int i = 0; i < config.Values.Count; i++)
            {
                object value = config.Values[i];

                string thisPath = path + (string.IsNullOrEmpty(path) ? string.Empty : ".") + i.ToString();

                if (value is ConfigObject child)
                {
                    writer.WriteLine(indent + "[" + level.ToString() + "] (" + thisPath + ") " + value.ToString());
                    WriteToFile(writer, child, level + 1, thisPath);
                }
                else if (value is string text)
                {
                    writer.WriteLine(indent + "[" + level.ToString() + "] (" + thisPath + ") \"" + text.ToString() + "\"");
                }
                else
                {
                    writer.WriteLine(indent + "[" + level.ToString() + "] (" + thisPath + ") " + value.ToString());
                }
            }
        }
    }
}