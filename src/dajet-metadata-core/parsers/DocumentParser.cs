using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class DocumentParser : IMetadataObjectParser
    {
        private readonly InfoBaseCache _cache;
        private ConfigFileParser _parser;
        private TablePartCollectionParser _tableParser;
        private MetadataPropertyCollectionParser _propertyParser;

        private Document _target;
        private MetadataEntry _entry;
        private ConfigFileConverter _converter;
        public DocumentParser(InfoBaseCache cache)
        {
            _cache = cache;
        }
        public void Parse(in ConfigFileReader source, out MetadataEntry target)
        {
            _entry = new MetadataEntry()
            {
                MetadataType = MetadataTypes.Document,
                MetadataUuid = new Guid(source.FileName)
            };

            _parser = new ConfigFileParser();
            _converter = new ConfigFileConverter();

            _converter[1][3] += Reference; // ДокументСсылка
            _converter[1][9][1][2] += Name; // Имя объекта метаданных конфигурации
            //TODO: _converter[1][24] += Registers; // Коллекция регистров движения документа [1][24]

            _parser.Parse(in source, in _converter);

            target = _entry;

            _entry = null;
            _parser = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader reader, out MetadataObject target)
        {
            _target = new Document()
            {
                Uuid = new Guid(reader.FileName)
            };

            ConfigureConverter();

            _parser = new ConfigFileParser();
            _tableParser = new TablePartCollectionParser();
            _propertyParser = new MetadataPropertyCollectionParser(_target);

            _parser.Parse(in reader, in _converter);

            // result
            target = _target;

            // dispose private variables
            _target = null;
            _parser = null;
            _converter = null;
            _tableParser = null;
            _propertyParser = null;
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
                _cache.AddReference(source.GetUuid(), _entry.MetadataUuid);
            }
        }
        private void Registers(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                args.Cancel = true;

                return;
            }

            //TODO !!!

            // 1.12.0 - UUID коллекции владельцев справочника !?
            // 1.12.1 - количество владельцев справочника
            // 1.12.N - описание владельцев
            // 1.12.N.2.1 - uuid'ы владельцев (file names)

            _ = source.Read(); // [1][12][0] - UUID коллекции владельцев справочника
            _ = source.Read(); // [1][12][1] - количество владельцев справочника

            int count = source.GetInt32();

            if (count == 0)
            {
                return;
            }

            int offset = 2; // начальный индекс N

            for (int n = 0; n < count; n++)
            {
                _converter[1][12][offset + n][2][1] += RegisterUuid;
            }
        }
        private void RegisterUuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (_entry != null)
            {
                _cache.AddDocumentRegister(_entry.MetadataUuid, source.GetUuid());
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