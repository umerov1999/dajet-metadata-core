using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class CatalogParser : IMetadataObjectParser
    {
        private ConfigFileParser _parser;
        private TablePartCollectionParser _tableParser;
        private MetadataPropertyCollectionParser _propertyParser;

        private Catalog _target;
        private ConfigFileConverter _converter;
        private Dictionary<MetadataProperty, List<Guid>> _references;
        public void Parse(in ConfigFileReader source, out MetadataObject target, out List<Guid> references)
        {
            throw new NotImplementedException();
        }
        public void Parse(in ConfigFileReader reader, out MetadataObject target, out Dictionary<MetadataProperty, List<Guid>> references)
        {
            _target = new Catalog()
            {
                Uuid = new Guid(reader.FileName) // [1][3]
            };
            _references = new Dictionary<MetadataProperty, List<Guid>>();

            ConfigureConverter();

            _parser = new ConfigFileParser();
            _tableParser = new TablePartCollectionParser();
            _propertyParser = new MetadataPropertyCollectionParser(_target);

            _parser.Parse(in reader, in _converter);

            // result
            target = _target;
            references = _references;

            // dispose private variables
            _target = null;
            _parser = null;
            _converter = null;
            _references = null;
            _tableParser = null;
            _propertyParser = null;
        }
        private void ConfigureConverter()
        {
            _converter = new ConfigFileConverter();

            _converter[1][9][1][2] += Name;
            _converter[1][9][1][3][2] += Alias;
            _converter[1][12][1] += Owners; // UUID объектов метаданных - владельцев справочника
            _converter[1][17] += CodeLength;
            _converter[1][18] += CodeType;
            _converter[1][19] += DescriptionLength;
            _converter[1][36] += HierarchyType;
            _converter[1][37] += IsHierarchical;

            _converter[5] += TablePartCollection; // 932159f9-95b2-4e76-a8dd-8849fe5c5ded - идентификатор коллекции табличных частей
            _converter[6] += PropertyCollection; // cf4abea7-37b2-11d4-940f-008048da11f9 - идентификатор коллекции реквизитов
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Name = source.Value;
        }
        private void Alias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Alias = source.Value;
        }
        private void Owners(in ConfigFileReader source, in CancelEventArgs args)
        {
            // 1.12.0 - UUID коллекции владельцев справочника ???
            // 1.12.1 - количество владельцев справочника
            // 1.12.N - описание владельцев
            // 1.12.N.2.1 - uuid'ы владельцев (file names)

            int count = source.GetInt32(); // [1][12][1]

            if (count == 0)
            {
                return; // THINK: добавить свойство "Владелец" ?
            }

            int offset = 2; // начальный индекс N

            for (int n = 0; n < count; n++)
            {
                _converter[1][12][offset + n][2][1] += OwnerUuid;
            }
        }
        private void OwnerUuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            // TODO: find property Владелец и заполнить DataTypeSet ?
            // Заранее, до вызова парсера, не известно есть ли свойство "Владелец"...
            _target.Owners.Add(source.GetUuid());
        }
        private void CodeType(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.CodeType = (CodeType)source.GetInt32();
        }
        private void CodeLength(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.CodeLength = source.GetInt32();
        }
        private void DescriptionLength(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.DescriptionLength = source.GetInt32();
        }
        private void HierarchyType(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.HierarchyType = (HierarchyType)source.GetInt32();
        }
        private void IsHierarchical(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.IsHierarchical = (source.GetInt32() != 0);
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