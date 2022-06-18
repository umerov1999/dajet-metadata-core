using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class CharacteristicParser : IMetadataObjectParser
    {
        private ConfigFileParser _parser;
        private DataTypeSetParser _typeParser;
        private TablePartCollectionParser _tableParser;
        private MetadataPropertyCollectionParser _propertyParser;

        private Characteristic _target;
        private ConfigFileConverter _converter;
        private Dictionary<MetadataProperty, List<Guid>> _references;
        public void Parse(in ConfigFileReader source, out MetadataObject target, out List<Guid> references)
        {
            throw new NotImplementedException();
        }
        public void Parse(in ConfigFileReader source, out MetadataObject target, out Dictionary<MetadataProperty, List<Guid>> references)
        {
            _target = new Characteristic()
            {
                Uuid = new Guid(source.FileName) // [1][3]
            };
            _references = new Dictionary<MetadataProperty, List<Guid>>();

            ConfigureConverter();

            _parser = new ConfigFileParser();
            _typeParser = new DataTypeSetParser();
            _tableParser = new TablePartCollectionParser();
            _propertyParser = new MetadataPropertyCollectionParser(_target);

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
            _tableParser = null;
            _propertyParser = null;
        }
        private void ConfigureConverter()
        {
            _converter = new ConfigFileConverter();

            _converter[1][9] += Reference;

            _converter[1][13][1][2] += Name;
            _converter[1][13][1][3][2] += Alias;
            _converter[1][18] += DataTypeSet;
            _converter[1][19] += IsHierarchical;
            _converter[1][21] += CodeLength;
            _converter[1][23] += DescriptionLength;

            _converter[3] += PropertyCollection; // 31182525-9346-4595-81f8-6f91a72ebe06 - идентификатор коллекции реквизитов
            _converter[5] += TablePartCollection; // 54e36536-7863-42fd-bea3-c5edd3122fdc - идентификатор коллекции табличных частей
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
        private void CodeLength(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.CodeLength = source.GetInt32();
        }
        private void DescriptionLength(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.DescriptionLength = source.GetInt32();
        }
        private void IsHierarchical(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.IsHierarchical = (source.GetInt32() != 0);
        }
        private void DataTypeSet(in ConfigFileReader source, in CancelEventArgs args)
        {
            // Корневой узел объекта "ОписаниеТипов"

            if (source.Token == TokenType.StartObject)
            {
                _typeParser.Parse(in source, out DataTypeSet type, out List<Guid> references);

                _target.DataTypeSet = type;
                
                //TODO: store references in DataTypeSet or _target !!! ?
            }
        }
        private void PropertyCollection(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.StartObject)
            {
                _propertyParser.Parse(in source, out List<MetadataProperty> properties, out Dictionary<MetadataProperty, List<Guid>> references);

                if (properties != null && properties.Count > 0)
                {
                    _target.Properties = properties;
                }

                if (references != null && references.Count > 0)
                {
                    foreach (KeyValuePair<MetadataProperty, List<Guid>> reference in references)
                    {
                        if (reference.Value != null && reference.Value.Count > 0)
                        {
                            _references.Add(reference.Key, reference.Value);
                        }
                    }
                }
            }
        }
        private void TablePartCollection(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.StartObject)
            {
                _tableParser.Parse(in source, out List<TablePart> tables);

                if (tables != null && tables.Count > 0)
                {
                    foreach (TablePart table in tables)
                    {
                        table.Owner = _target;
                    }

                    _target.TableParts = tables;
                }
            }
        }
    }
}