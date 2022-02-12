using System;

namespace DaJet.Metadata.Core
{
    public readonly struct ReferenceInfo
    {
        internal ReferenceInfo(Guid uuid, string name)
        {
            Uuid = uuid;
            Name = name;
            CharacteristicUuid = Guid.Empty;
        }
        internal ReferenceInfo(Guid uuid, string name, Guid chrcUuid)
        {
            Uuid = uuid;
            Name = name;
            CharacteristicUuid = chrcUuid;
        }
        public readonly Guid Uuid { get; }
        public readonly string Name { get; }
        public readonly Guid CharacteristicUuid { get; }
    }
}