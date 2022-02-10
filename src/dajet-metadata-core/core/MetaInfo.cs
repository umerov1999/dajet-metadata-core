using System;

namespace DaJet.Metadata.Core
{
    public readonly struct MetaInfo
    {
        internal MetaInfo(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
        }
        public readonly Guid Uuid { get; }
        public readonly string Name { get; }
    }
}