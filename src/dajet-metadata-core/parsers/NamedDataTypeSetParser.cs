using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class NamedDataTypeSetParser : IMetadataObjectParser
    {
        private ConfigFileParser _parser;
        private DataTypeSetParser _typeParser;

        private List<Guid> _references;
        private NamedDataTypeSet _target;
        private ConfigFileConverter _converter;
        public void Parse(in ConfigFileReader source, out MetadataObject target, out List<Guid> references)
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
        public void Parse(in ConfigFileReader source, out MetadataObject target, out Dictionary<MetadataProperty, List<Guid>> references)
        {
            throw new NotImplementedException();
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
            _target.Reference = source.GetUuid();
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Name = source.Value;
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