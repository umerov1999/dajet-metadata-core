using System;

namespace DaJet.Metadata.Core
{
    public class MetadataEntity
    {
        public Guid Type { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
    }
}