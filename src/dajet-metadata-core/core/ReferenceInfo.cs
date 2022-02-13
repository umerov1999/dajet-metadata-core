using System;

namespace DaJet.Metadata.Core
{
    public readonly struct ReferenceInfo
    {
        internal ReferenceInfo(Guid type, Guid metadata, Guid reference)
        {
            MetadataType = type;
            MetadataUuid = metadata;
            ReferenceUuid = reference;
            CharacteristicUuid = Guid.Empty;
        }
        internal ReferenceInfo(Guid type, Guid metadata, Guid reference, Guid characteristic)
        {
            MetadataType = type;
            MetadataUuid = metadata;
            ReferenceUuid = reference;
            CharacteristicUuid = characteristic;
        }
        public readonly Guid MetadataType { get; }
        public readonly Guid MetadataUuid { get; }
        public readonly Guid ReferenceUuid { get; }
        public readonly Guid CharacteristicUuid { get; }
    }
}