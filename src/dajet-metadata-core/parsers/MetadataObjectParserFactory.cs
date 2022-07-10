using DaJet.Metadata.Core;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Parsers
{
    public sealed class MetadataObjectParserFactory
    {
        private readonly MetadataCache _cache;
        private readonly Dictionary<Guid, Func<IMetadataObjectParser>> _parsers;
        public MetadataObjectParserFactory(MetadataCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _parsers = new() // supported metadata object parsers
            {
                { MetadataTypes.Catalog, CreateCatalogParser },
                { MetadataTypes.Document, CreateDocumentParser },
                { MetadataTypes.Enumeration, CreateEnumerationParser },
                { MetadataTypes.Publication, CreatePublicationParser },
                { MetadataTypes.Characteristic, CreateCharacteristicParser },
                { MetadataTypes.InformationRegister, CreateInformationRegisterParser },
                { MetadataTypes.AccumulationRegister, CreateAccumulationRegisterParser },
                { MetadataTypes.SharedProperty, CreateSharedPropertyParser },
                { MetadataTypes.NamedDataTypeSet, CreateNamedDataTypeSetParser } // since 1C:Enterprise 8.3.3 version
            };
        }
        public bool TryCreateParser(Guid type, out IMetadataObjectParser parser)
        {
            parser = null;

            if (!_parsers.TryGetValue(type, out Func<IMetadataObjectParser> factory))
            {
                return false; // Unsupported metadata type
            }

            parser = factory();

            return true;
        }
        private IMetadataObjectParser CreateCatalogParser()
        {
            return new CatalogParser(_cache);
        }
        private IMetadataObjectParser CreateDocumentParser()
        {
            return new DocumentParser(_cache);
        }
        private IMetadataObjectParser CreateEnumerationParser()
        {
            return new EnumerationParser(_cache);
        }
        private IMetadataObjectParser CreatePublicationParser()
        {
            return new PublicationParser(_cache);
        }
        private IMetadataObjectParser CreateCharacteristicParser()
        {
            return new CharacteristicParser(_cache);
        }
        private IMetadataObjectParser CreateInformationRegisterParser()
        {
            return new InformationRegisterParser(_cache);
        }
        private IMetadataObjectParser CreateAccumulationRegisterParser()
        {
            return new AccumulationRegisterParser(_cache);
        }
        private IMetadataObjectParser CreateSharedPropertyParser()
        {
            return new SharedPropertyParser(_cache);
        }
        private IMetadataObjectParser CreateNamedDataTypeSetParser()
        {
            return new NamedDataTypeSetParser(_cache);
        }
    }
}