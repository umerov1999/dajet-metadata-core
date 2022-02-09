using DaJet.Metadata.Core;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Parsers
{
    public static class MetadataParserFactory
    {
        private static readonly Dictionary<Guid, IMetadataObjectParser> _parsers = new Dictionary<Guid, IMetadataObjectParser>();
        static MetadataParserFactory()
        {
            _parsers.Add(MetadataTypes.Subsystem, null); // Подсистемы
            _parsers.Add(MetadataTypes.NamedDataTypeSet, new NamedDataTypeSetParser()); // Определяемые типы
            _parsers.Add(MetadataTypes.SharedProperty, null); // Общие реквизиты
            _parsers.Add(MetadataTypes.Catalog, null);
            _parsers.Add(MetadataTypes.Constant, null);
            _parsers.Add(MetadataTypes.Document, null);
            _parsers.Add(MetadataTypes.Enumeration, null);
            _parsers.Add(MetadataTypes.Publication, null); // Планы обмена
            _parsers.Add(MetadataTypes.Characteristic, null);
            _parsers.Add(MetadataTypes.InformationRegister, new InformationRegisterParser());
            _parsers.Add(MetadataTypes.AccumulationRegister, null);
        }
        public static bool TryGetParser(Guid type, out IMetadataObjectParser parser)
        {
            return _parsers.TryGetValue(type, out parser);
        }
    }
}