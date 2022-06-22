using System;

namespace DaJet.Metadata.Core
{
    internal readonly struct MetadataEntry
    {
        internal MetadataEntry(Guid type, Guid uuid)
        {
            MetadataType = type;
            MetadataUuid = uuid;
        }
        internal Guid MetadataType { get; } = Guid.Empty;
        internal Guid MetadataUuid { get; } = Guid.Empty;
    }
}