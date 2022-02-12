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

        private string _name;
        private NamedDataTypeSet _target;
        private ConfigFileConverter _converter;
        public void Parse(in ConfigFileReader source, out MetadataObject target)
        {
            Parse(in source, null, out target);
        }
        public void Parse(in ConfigFileReader source, in string name, out MetadataObject target)
        {
            _target = new NamedDataTypeSet()
            {
                Uuid = new Guid(source.FileName)
            };

            ConfigureConverter(in source);

            _typeParser = new DataTypeSetParser();

            _name = name; // filter

            _parser = new ConfigFileParser();
            _parser.Parse(in source, in _converter);

            target = _target; // result

            // dispose private variables
            _name = null;
            _target = null;
            _parser = null;
            _converter = null;
            _typeParser = null;
        }
        private void ConfigureConverter(in ConfigFileReader source)
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
            // apply filter by name
            if (_name != null && _name != source.Value)
            {
                _target = null;
                args.Cancel = true;
                return;
            }

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
                _typeParser.Parse(in source, out DataTypeSet type, out List<Guid> references);

                _target.DataTypeSet = type; // FIXME: выполнить преобразование references !!!
            }
        }
    }
}