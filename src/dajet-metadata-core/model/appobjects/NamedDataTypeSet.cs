using System;

namespace DaJet.Metadata.Model
{
    public sealed class NamedDataTypeSet : MetadataObject
    {
        public Guid Reference { get; set; } = Guid.Empty;
        public DataTypeSet DataTypeSet { get; set; }
    }
}