using System;

namespace DaJet.Metadata.Core
{
    public readonly struct MetadataItem
    {
        public static MetadataItem Empty { get; } = new();
        internal MetadataItem(Guid type, Guid uuid)
        {
            Type = type;
            Uuid = uuid;
        }
        internal MetadataItem(Guid type, Guid uuid, string name) : this(type, uuid)
        {
            Name = name;
        }
        public Guid Type { get; } = Guid.Empty;
        public Guid Uuid { get; } = Guid.Empty;
        public string Name { get; } = string.Empty;

        #region " Переопределение методов сравнения "

        public override int GetHashCode()
        {
            return Uuid.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) { return false; }

            if (obj is not MetadataItem test)
            {
                return false;
            }

            return (this == test);
        }
        public static bool operator ==(MetadataItem left, MetadataItem right)
        {
            return left.Type == right.Type
                && left.Uuid == right.Uuid;
        }
        public static bool operator !=(MetadataItem left, MetadataItem right)
        {
            return !(left == right);
        }

        #endregion
    }
}