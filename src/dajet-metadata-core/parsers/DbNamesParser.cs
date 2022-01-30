using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class DbNamesParser
    {
        private readonly ConfigFileParser _parser = new ConfigFileParser();
        private ConfigFileReader Reader { get; }
        public DbNamesParser(ConfigFileReader reader)
        {
            Reader = reader;
            ConfigureConfigFileConverter();
        }
        public void Parse(in DbNames target)
        {
            _lookup = target;
            _parser.Parse(Reader, _converter);
        }

        int _count = 0;
        DbNames _lookup;
        ConfigFileConverter _converter;
        private void ConfigureConfigFileConverter()
        {
            _converter = new ConfigFileConverter();
            _converter[1][0] += ReadDbNameItems; // Количество элементов файла DbNames
        }
        private void ReadDbNameItems(in ConfigFileReader source, in CancelEventArgs args)
        {
            _count = source.GetInt32();

            int code;
            Guid uuid;
            string name;

            while (_count > 0)
            {
                if (!source.Read()) { break; }

                if (source.Token == TokenType.StartObject)
                {
                    uuid = ReadUuid(in source); // 1.x.0 - uuid
                    name = ReadName(in source); // 1.x.1 - name
                    code = ReadCode(in source); // 1.x.2 - code

                    if (code > 0 && uuid != Guid.Empty && name != null)
                    {
                        _lookup.Add(code, uuid, name);
                    }
                }
                else if (source.Token == TokenType.EndObject)
                {
                    _count--;
                }
            }
        }
        private Guid ReadUuid(in ConfigFileReader source)
        {
            if (source.Read()) 
            {
                return source.GetUuid();
            }
            return Guid.Empty;
        }
        private string ReadName(in ConfigFileReader source)
        {
            if (source.Read())
            {
                return source.Value;
            }
            return null;
        }
        private int ReadCode(in ConfigFileReader source)
        {
            if (source.Read())
            {
                return source.GetInt32();
            }
            return -1;
        }
    }
}