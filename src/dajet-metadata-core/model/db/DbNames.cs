using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Model
{
    public sealed class DbName // TODO: make struct !?
    {
        public int Code { get; set; } // Unique code
        public Guid Uuid { get; set; } // Metadata object uuid (not unique because of duplicates in service items)
        public string Name { get; set; } // Prefix of the database object name
        public List<DbName> ServiceItems { get; } = new List<DbName>(); // VT + LineNo | Reference + ReferenceChngR
        public override string ToString()
        {
            return $"{Name} {{{Code}:{Uuid}}}";
        }
    }
    public sealed class DbNames // DbNameCache
    {
        private readonly HashSet<string> _main = new HashSet<string>()
        {
            MetadataTokens.VT,
            MetadataTokens.Acc,
            MetadataTokens.Chrc,
            MetadataTokens.Enum,
            MetadataTokens.Node,
            MetadataTokens.Const,
            MetadataTokens.AccRg,
            MetadataTokens.InfoRg,
            MetadataTokens.AccumRg,
            MetadataTokens.Document,
            MetadataTokens.Reference
        };

        private readonly Dictionary<Guid, DbName> _lookup = new Dictionary<Guid, DbName>();
        public Dictionary<Guid, DbName> Lookup { get { return _lookup; } }

        public void Add(int code, Guid uuid, string name)
        {
            if (_lookup.TryGetValue(uuid, out DbName entry))
            {
                if (_main.Contains(name))
                {
                    entry.Code = code;
                    entry.Name = name;
                }
                else
                {
                    entry.ServiceItems.Add(new DbName()
                    {
                        Uuid = uuid,
                        Code = code,
                        Name = name
                    });
                }
            }
            else
            {
                DbName item = new DbName()
                {
                    Uuid = uuid,
                    Code = code,
                    Name = name
                };

                if (!_main.Contains(name))
                {
                    item.ServiceItems.Add(new DbName()
                    {
                        Uuid = uuid,
                        Code = code,
                        Name = name
                    });
                }

                _lookup.Add(uuid, item);
            }
        }
    }
}