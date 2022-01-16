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
            _parsers.Add(MetadataRegistry.Root, new InfoBaseParser());
            _parsers.Add(MetadataRegistry.Subsystems, null); // Подсистемы
            _parsers.Add(MetadataRegistry.NamedDataTypeSets, null); // Определяемые типы
            _parsers.Add(MetadataRegistry.SharedProperties, null); // Общие реквизиты
            _parsers.Add(MetadataRegistry.Catalogs, null);
            _parsers.Add(MetadataRegistry.Constants, null);
            _parsers.Add(MetadataRegistry.Documents, null);
            _parsers.Add(MetadataRegistry.Enumerations, null);
            _parsers.Add(MetadataRegistry.Publications, null); // Планы обмена
            _parsers.Add(MetadataRegistry.Characteristics, null);
            _parsers.Add(MetadataRegistry.InformationRegisters, new InformationRegisterParser());
            _parsers.Add(MetadataRegistry.AccumulationRegisters, null);
        }
        public static bool TryGetParser(Guid type, out IMetadataObjectParser parser)
        {
            return _parsers.TryGetValue(type, out parser);
        }
    }
}