using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class AccumulationRegisterParser : IMetadataObjectParser
    {
        private readonly InfoBaseCache _cache;
        private ConfigFileParser _parser;

        private MetadataEntry _entry;
        private AccumulationRegister _target;
        private ConfigFileConverter _converter;
        public AccumulationRegisterParser(InfoBaseCache cache)
        {
            _cache = cache;
        }
        public void Parse(in ConfigFileReader source, out MetadataEntry target)
        {
            _entry = new MetadataEntry()
            {
                MetadataType = MetadataTypes.AccumulationRegister,
                MetadataUuid = new Guid(source.FileName)
            };

            _parser = new ConfigFileParser();
            _converter = new ConfigFileConverter();

            _converter[1][13][1][2] += Name; // Имя объекта конфигурации

            _parser.Parse(in source, in _converter);

            target = _entry;

            _entry = null;
            _parser = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader reader, out MetadataObject target)
        {
            _target = new AccumulationRegister()
            {
                Uuid = new Guid(reader.FileName)
            };

            ConfigureConverter();

            _parser = new ConfigFileParser();

            _parser.Parse(in reader, in _converter);

            // result
            target = _target;

            // dispose private variables
            _target = null;
            _parser = null;
            _converter = null;
        }
        private void ConfigureConverter()
        {
            _converter = new ConfigFileConverter();

            //TODO

            _converter[1][13][1][2] += Name;
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (_entry != null)
            {
                _entry.Name = source.Value;
                args.Cancel = true;
                return;
            }

            if (_target != null)
            {
                _target.Name = source.Value;
            }
        }
    }
}