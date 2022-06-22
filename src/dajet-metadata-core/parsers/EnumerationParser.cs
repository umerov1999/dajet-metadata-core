using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class EnumerationParser : IMetadataObjectParser
    {
        private readonly MetadataCache _cache;
        private ConfigFileParser _parser;

        private Enumeration _target;
        private MetadataInfo _entry;
        private ConfigFileConverter _converter;
        public EnumerationParser(MetadataCache cache)
        {
            _cache = cache;
        }
        public void Parse(in ConfigFileReader source, out MetadataInfo target)
        {
            _entry = new MetadataInfo()
            {
                MetadataType = MetadataTypes.Enumeration,
                MetadataUuid = new Guid(source.FileName)
            };

            _parser = new ConfigFileParser();
            _converter = new ConfigFileConverter();

            _converter[1][1] += Reference; // Идентификатор ссылочного типа данных
            _converter[1][5][1][2] += Name; // Имя объекта конфигурации

            _parser.Parse(in source, in _converter);

            target = _entry;

            _entry = null;
            _parser = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader reader, out MetadataObject target)
        {
            _target = new Enumeration()
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

            //_converter[1][9][1][2] += Name;
            //_converter[1][9][1][3][2] += Alias;

            //_converter[5] += TablePartCollection;
            //_converter[6] += PropertyCollection;
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
        private void Reference(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (_entry != null)
            {
                _entry.ReferenceUuid = source.GetUuid();
            }
        }
    }
}