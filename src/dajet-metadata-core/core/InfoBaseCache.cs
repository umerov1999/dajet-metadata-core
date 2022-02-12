using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaJet.Metadata.Core
{
    internal sealed class InfoBaseCache
    {
        private readonly string _connectionString;
        private readonly DatabaseProvider _provider;

        private readonly Dictionary<Guid, IMetadataObjectParser> _parsers = new()
        {
            { MetadataTypes.InformationRegister, new InformationRegisterParser() }
        };

        private Guid _root = Guid.Empty;
        private ConcurrentDictionary<Guid, Guid> _references;
        private ConcurrentDictionary<Guid, Dictionary<string, Guid>> _names;
        private ConcurrentDictionary<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> _cache;
        
        //private DbNames _data;
        //private Dictionary<int, Guid> _codes;

        internal InfoBaseCache(DatabaseProvider provider, in string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
        }

        internal void Initialize(out InfoBase infoBase)
        {
            InitializeRootFile();
            InitializeMetadataCache(out infoBase);
            InitializeReferenceCache();
        }
        private void InitializeRootFile()
        {
            using (ConfigFileReader reader = new ConfigFileReader(_provider, in _connectionString, ConfigTables.Config, ConfigFiles.Root))
            {
                _root = new RootFileParser().Parse(in reader);
            }
        }
        private void InitializeMetadataCache(out InfoBase infoBase)
        {
            using (ConfigFileReader reader = new ConfigFileReader(_provider, in _connectionString, ConfigTables.Config, _root))
            {
                new InfoBaseParser().Parse(in reader, out infoBase, out _cache);
            }
        }
        private void InitializeReferenceCache()
        {
            _names = new ConcurrentDictionary<Guid, Dictionary<string, Guid>>();
            _references = new ConcurrentDictionary<Guid, Guid>();

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(_cache, options, InitializeReferenceTypeCache);
        }
        private void InitializeReferenceTypeCache(KeyValuePair<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> cache)
        {
            Guid type = cache.Key;

            if (type == MetadataTypes.Constant ||
                type == MetadataTypes.SharedProperty)
            {
                return;
            }

            if (!(type == MetadataTypes.Catalog ||
                type == MetadataTypes.Document ||
                type == MetadataTypes.Enumeration ||
                type == MetadataTypes.Publication ||
                type == MetadataTypes.Characteristic ||
                type == MetadataTypes.NamedDataTypeSet ||
                type == MetadataTypes.InformationRegister ||
                type == MetadataTypes.AccumulationRegister))
            {
                return;
            }

            ReferenceParser parser = new();

            foreach (var entry in cache.Value)
            {
                if (entry.Key == Guid.Empty)
                {
                    continue;
                }

                using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Config, entry.Key))
                {
                    ReferenceInfo reference = parser.Parse(in reader, type);

                    if (reference.Name == string.Empty)
                    {
                        continue; // accidentally: unsupported metadata object type
                    }

                    if (reference.Uuid != Guid.Empty)
                    {
                        // reference type uuid to metadata object uuid mapping
                        if (type == MetadataTypes.Characteristic)
                        {
                            _ = _references.TryAdd(reference.CharacteristicUuid, entry.Key);
                        }
                        _ = _references.TryAdd(reference.Uuid, entry.Key);
                    }

                    // metadata object name to uuid mapping
                    if (_names.TryGetValue(type, out Dictionary<string, Guid> names))
                    {
                        names.Add(reference.Name, entry.Key);
                    }
                    else
                    {
                        _ = _names.TryAdd(type, new Dictionary<string, Guid>() { { reference.Name, entry.Key } });
                    }
                }
            }
        }

        internal void GetMetadataObject(Guid type, Guid uuid, out MetadataObject metadata)
        {
            if (!_parsers.TryGetValue(type, out IMetadataObjectParser parser))
            {
                throw new InvalidOperationException(); // this should not happen
            }

            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata);
            }
        }
        internal MetadataObject GetMetadataObjectCached(Guid type, Guid uuid)
        {
            if (!_cache.TryGetValue(type, out Dictionary<Guid, WeakReference<MetadataObject>> entry))
            {
                return null;
            }

            if (!entry.TryGetValue(uuid, out WeakReference<MetadataObject> reference))
            {
                return null;
            }

            if (!reference.TryGetTarget(out MetadataObject metadata))
            {
                UpdateMetadataObjectCache(type, uuid, out metadata);
            }

            return metadata;
        }
        internal MetadataObject GetMetadataObjectCached(in string typeName, in string objectName)
        {
            Guid type = MetadataTypes.ResolveName(typeName);

            if (type == Guid.Empty)
            {
                return null;
            }

            if (!_names.TryGetValue(type, out Dictionary<string, Guid> names))
            {
                return null;
            }

            if (!names.TryGetValue(objectName, out Guid uuid))
            {
                return null;
            }

            return GetMetadataObjectCached(type, uuid);
        }
        private void UpdateMetadataObjectCache(Guid type, Guid uuid, out MetadataObject metadata)
        {
            if (!_cache.TryGetValue(type, out Dictionary<Guid, WeakReference<MetadataObject>> entry))
            {
                throw new InvalidOperationException(); // this should not happen
            }

            if (!entry.TryGetValue(uuid, out WeakReference<MetadataObject> reference))
            {
                throw new InvalidOperationException(); // this should not happen
            }

            GetMetadataObject(type, uuid, out metadata);

            reference.SetTarget(metadata);
        }
    }
}