using DaJet.Metadata.Model;
using System.ComponentModel;

namespace DaJet.Metadata.Core
{
    public delegate void ConfigFileTokenHandler(in ConfigFileReader source, in CancelEventArgs args);

    public static class ConfigFileParser
    {
        private static readonly CancelEventArgs _args = new CancelEventArgs(false);
        public static ConfigObject Parse(in ConfigFileReader reader)
        {
            ConfigObject config = new ConfigObject();
            ParseFile(in config, in reader);
            return config;
        }
        private static void ParseFile(in ConfigObject parent, in ConfigFileReader reader)
        {
            while (reader.Read())
            {
                if (reader.Token == TokenType.StartFile)
                {
                    continue;
                }
                else if (reader.Token == TokenType.StartObject)
                {
                    ConfigObject config = new ConfigObject();
                    ParseFile(in config, in reader);
                    parent.Values.Add(config);
                }
                else if (reader.Token == TokenType.EndObject)
                {
                    return;
                }
                else if (reader.Token == TokenType.EndFile)
                {
                    return;
                }
                else
                {
                    parent.Values.Add(reader.Value);
                }
            }
        }

        public static void Parse(in ConfigFileReader source, in ConfigFileConverter converter)
        {
            _args.Cancel = false;

            ConfigFileConverter current = converter;

            while (source.Read())
            {
                if (source.Token == TokenType.StartFile || source.Token == TokenType.EndFile)
                {
                    converter.TokenHandler?.Invoke(in source, in _args);
                    if (_args.Cancel) { break; }
                }
                else if (source.Token == TokenType.StartObject)
                {
                    current = current[source.ValuePointer];
                    current.TokenHandler?.Invoke(in source, in _args);
                    if (_args.Cancel) { break; }
                }
                else if (source.Token == TokenType.EndObject)
                {
                    current.TokenHandler?.Invoke(in source, in _args);
                    if (_args.Cancel) { break; }
                    current = current.Parent;
                }
                else if (source.Token == TokenType.Value || source.Token == TokenType.String)
                {
                    current[source.ValuePointer].TokenHandler?.Invoke(in source, in _args);
                    if (_args.Cancel) { break; }
                }
            }
        }
    }
}