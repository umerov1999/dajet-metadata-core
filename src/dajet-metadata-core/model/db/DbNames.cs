using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Model
{
    public readonly struct DbName
    {
        internal DbName(Guid uuid, int code, string name)
        {
            Uuid = uuid;
            Code = code;
            Name = name;
        }
        public readonly int Code { get; } // Unique code
        public readonly Guid Uuid { get; } // Metadata object uuid (not unique because of duplicates in child entries)
        public readonly string Name { get; } // Prefix of the database object name
        public readonly List<DbName> Children { get; } = new List<DbName>(); // VT + LineNo | Reference + ReferenceChngR
        public override string ToString()
        {
            return $"{Name} {{{Code}:{Uuid}}}";
        }
    }
    public sealed class DbNameCache
    {
        private readonly Dictionary<Guid, DbName> _cache = new();
        public IEnumerable<DbName> DbNames
        {
            get { return _cache.Values; }
        }
        public bool TryGet(Guid uuid, out DbName entry)
        {
            return _cache.TryGetValue(uuid, out entry);
        }
        public void Add(Guid uuid, int code, string name)
        {
            // NOTE: the case when child and parent items are in the wrong order is not assumed

            if (_cache.TryGetValue(uuid, out DbName entry))
            {
                entry.Children.Add(new DbName(uuid, code, name));
            }
            else
            {
                _cache.Add(uuid, new DbName(uuid, code, name));
            }
        }
    }
}