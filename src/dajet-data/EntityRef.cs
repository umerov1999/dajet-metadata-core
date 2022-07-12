namespace DaJet.Data
{
    public readonly struct EntityRef
    {
        public static EntityRef Empty { get; } = new();
        public EntityRef(int typeCode, Guid identity)
        {
            TypeCode = typeCode;
            Identity = identity;
        }
        public int TypeCode { get; } = 0;
        public Guid Identity { get; } = Guid.Empty;
        public override string ToString()
        {
            return $"{{{TypeCode}:{Identity}}}";
        }

        #region " Переопределение методов сравнения "

        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) { return false; }

            if (obj is not EntityRef test)
            {
                return false;
            }

            return (this == test);
        }
        public static bool operator ==(EntityRef left, EntityRef right)
        {
            return left.TypeCode == right.TypeCode
                && left.Identity == right.Identity;
        }
        public static bool operator !=(EntityRef left, EntityRef right)
        {
            return !(left == right);
        }

        #endregion
    }
}