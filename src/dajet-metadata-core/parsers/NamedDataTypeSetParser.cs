using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class NamedDataTypeSetParser : IMetadataObjectParser
    {
        private readonly InfoBaseCache _cache;
        private ConfigFileParser _parser;
        private DataTypeSetParser _typeParser;

        private List<Guid> _references; // TODO: remove - configure DataTypeSet
        private MetadataEntry _entry;
        private NamedDataTypeSet _target;
        private ConfigFileConverter _converter;
        public NamedDataTypeSetParser(InfoBaseCache cache)
        {
            _cache = cache;
        }
        public void Parse(in ConfigFileReader source, out MetadataEntry target)
        {
            _entry = new MetadataEntry()
            {
                MetadataType = MetadataTypes.NamedDataTypeSet,
                MetadataUuid = new Guid(source.FileName)
            };

            _parser = new ConfigFileParser();
            _converter = new ConfigFileConverter();

            _converter[1][1] += Reference; // Идентификатор ссылочного типа данных "Ссылка"
            _converter[1][3][2] += Name; // Имя объекта конфигурации

            _parser.Parse(in source, in _converter);

            target = _entry;

            _entry = null;
            _parser = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader source, out MetadataObject target)
        {
            _target = new NamedDataTypeSet()
            {
                Uuid = new Guid(source.FileName)
            };
            
            ConfigureConverter();

            _typeParser = new DataTypeSetParser();

            _parser = new ConfigFileParser();
            _parser.Parse(in source, in _converter);

            // result
            target = _target;
            references = _references;

            // dispose private variables
            _target = null;
            _parser = null;
            _converter = null;
            _references = null;
            _typeParser = null;
        }
        private void ConfigureConverter()
        {
            _converter = new ConfigFileConverter();

            _converter[1][1] += Reference;
            _converter[1][3][2] += Name;
            _converter[1][3][3][2] += Alias;
            _converter[1][3][4] += Comment;
            _converter[1][4] += DataTypeSet;
        }
        private void Reference(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (_entry != null)
            {
                _cache.AddReference(source.GetUuid(), _entry.MetadataUuid);

                return;
            }

            if (_target != null)
            {
                _target.Reference = source.GetUuid(); // TODO: remove ?
            }
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
        private void Alias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Alias = source.Value;
        }
        private void Comment(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Comment = source.Value;
        }
        private void DataTypeSet(in ConfigFileReader source, in CancelEventArgs args)
        {
            // Корневой узел объекта "ОписаниеТипов"

            if (source.Token == TokenType.StartObject)
            {
                _typeParser.Parse(in source, out DataTypeSet type, out _references);

                _target.DataTypeSet = type;
            }
        }
    }
}