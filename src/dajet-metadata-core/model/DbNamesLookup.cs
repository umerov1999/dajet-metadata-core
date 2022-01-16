using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Model
{
    public sealed class DbNameItem
    {
        public int Code { get; set; } // Unique code
        public Guid Uuid { get; set; } // File name (not unique because of duplicates in service items)
        public string DbName { get; set; } // Prefix of the database object name
        public List<DbNameItem> ServiceItems { get; } = new List<DbNameItem>(); // VT + LineNo | Reference + ReferenceChngR
        public override string ToString()
        {
            return $"{DbName} {{{Code}:{Uuid}}}";
        }
    }
    public sealed class DbNamesLookup
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

        private readonly Dictionary<Guid, DbNameItem> _lookup = new Dictionary<Guid, DbNameItem>();
        public Dictionary<Guid, DbNameItem> Lookup { get { return _lookup; } }

        public void Add(int code, Guid uuid, string name)
        {
            if (_lookup.TryGetValue(uuid, out DbNameItem entry))
            {
                if (_main.Contains(name))
                {
                    entry.Code = code;
                    entry.DbName = name;
                }
                else
                {
                    entry.ServiceItems.Add(new DbNameItem()
                    {
                        Uuid = uuid,
                        Code = code,
                        DbName = name
                    });
                }
            }
            else
            {
                DbNameItem item = new DbNameItem()
                {
                    Uuid = uuid,
                    Code = code,
                    DbName = name
                };

                if (!_main.Contains(name))
                {
                    item.ServiceItems.Add(new DbNameItem()
                    {
                        Uuid = uuid,
                        Code = code,
                        DbName = name
                    });
                }

                _lookup.Add(uuid, item);
            }
        }
    }
}