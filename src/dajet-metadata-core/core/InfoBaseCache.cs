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
        private readonly NamedDataTypeSetParser _namedDataTypeSetParser = new();

        private Guid _root = Guid.Empty;
        private ConcurrentDictionary<Guid, ReferenceInfo> _references;
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
            _references = new ConcurrentDictionary<Guid, ReferenceInfo>();

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
                    ReferenceInfo reference = parser.Parse(in reader, type, out string name);

                    if (string.IsNullOrEmpty(name))
                    {
                        continue; // accidentally: unsupported metadata object type
                    }

                    // metadata object name to uuid mapping
                    if (_names.TryGetValue(type, out Dictionary<string, Guid> names))
                    {
                        names.Add(name, entry.Key);
                    }
                    else
                    {
                        _ = _names.TryAdd(type, new Dictionary<string, Guid>() { { name, entry.Key } });
                    }

                    // reference type uuid to metadata object mapping
                    if (reference.ReferenceUuid != Guid.Empty)
                    {
                        if (reference.MetadataType == MetadataTypes.Characteristic)
                        {
                            _ = _references.TryAdd(reference.CharacteristicUuid, reference);
                        }
                        _ = _references.TryAdd(reference.ReferenceUuid, reference);
                    }
                }
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

        internal void GetMetadataObject(Guid type, Guid uuid, out MetadataObject metadata)
        {
            if (type == MetadataTypes.NamedDataTypeSet)
            {
                // since 1C:Enterprise 8.3.3 version
                GetNamedDataTypeSet(uuid, out metadata);
            }
            else
            {
                GetApplicationObject(type, uuid, out metadata);
            }
        }
        private void GetNamedDataTypeSet(Guid uuid, out MetadataObject metadata)
        {
            List<Guid> references;
            NamedDataTypeSet namedDataTypeSet;

            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                _namedDataTypeSetParser.Parse(in reader, out namedDataTypeSet, out references);
            }

            ConfigureDataTypeSet(namedDataTypeSet.DataTypeSet, in references);

            metadata = namedDataTypeSet;
        }
        private void GetApplicationObject(Guid type, Guid uuid, out MetadataObject metadata)
        {
            if (!_parsers.TryGetValue(type, out IMetadataObjectParser parser))
            {
                throw new InvalidOperationException(); // this should not happen
            }

            Dictionary<MetadataProperty, List<Guid>> references;

            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata, out references);
            }

            if (references != null && references.Count > 0)
            {
                ConfigureMetadataProperties(in metadata, in references);
            }

            // TODO:
            // Состав системных свойств регистра сведений зависит от прочитанных метаданных
            // Configurator.ConfigureInformationRegister(in _target, reader.DatabaseProvider);

            // TODO:
            // Configurator.ConfigureSharedProperties(register);
        }
        private void ConfigureMetadataProperties(in MetadataObject metadata, in Dictionary<MetadataProperty, List<Guid>> references)
        {
            if (metadata is not ApplicationObject entity)
            {
                return;
            }

            foreach (MetadataProperty property in entity.Properties)
            {
                if (references.TryGetValue(property, out List<Guid> referenceTypes))
                {
                    ConfigureDataTypeSet(property.PropertyType, in referenceTypes);
                }
            }
        }
        private void ConfigureDataTypeSet(in DataTypeSet target, in List<Guid> references)
        {
            if (references == null || references.Count == 0)
            {
                return;
            }

            if (references.Count > 1)
            {
                target.CanBeReference = true;
                return;
            }

            // TODO: loop through the list of references
            Guid reference = references[0];

            if (reference == Guid.Empty)
            {
                return;
            }

            if (reference == ReferenceTypes.AnyReference)
            {
                // TODO !!!
                return;
            }

            if (!_references.TryGetValue(reference, out ReferenceInfo info))
            {
                return; // this should not happen
            }

            if (info.MetadataType == MetadataTypes.NamedDataTypeSet ||
                (info.MetadataType == MetadataTypes.Characteristic && reference == info.CharacteristicUuid))
            {
                MetadataObject metadata = GetMetadataObjectCached(info.MetadataType, info.MetadataUuid);

                if (metadata is NamedDataTypeSet source)
                {
                    target.Apply(source.DataTypeSet);
                }
                else if (metadata is Characteristic characteristic)
                {
                    target.Apply(characteristic.TypeInfo);
                }
            }
            else
            {
                target.CanBeReference = true;
                target.Reference = reference; // single reference type
            }
        }
    }
}