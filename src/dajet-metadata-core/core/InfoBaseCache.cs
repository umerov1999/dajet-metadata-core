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
            { MetadataTypes.Catalog, new CatalogParser() },
            { MetadataTypes.Document, null },
            { MetadataTypes.Enumeration, null },
            { MetadataTypes.Publication, null },
            { MetadataTypes.Characteristic, new CharacteristicParser() },
            { MetadataTypes.InformationRegister, new InformationRegisterParser() },
            { MetadataTypes.AccumulationRegister, null },
            { MetadataTypes.SharedProperty, new SharedPropertyParser() },
            { MetadataTypes.NamedDataTypeSet, new NamedDataTypeSetParser() } // since 1C:Enterprise 8.3.3 version
        };

        #region "PRIVATE CACHE VALUES"

        // Корневой файл конфигурации
        private Guid _root = Guid.Empty;
        
        // Общий тип метаданных + тип объекта метаданных + подробное описание объекта метаданных
        // Например, Справочник.Номенклатура
        private ConcurrentDictionary<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> _cache;

        // UUID типа данных "Ссылка", например, "СправочникСсылка.Номенклатура" + структура дополнительной информации
        private ConcurrentDictionary<Guid, ReferenceInfo> _references;

        // Общий тип метаданных + имя объекта метаданных + тип объекта метаданных
        // Коллекция используется для поиска объекта метаданных по его полному имени
        // Например, Справочник.Номенклатура = UUID объекта метаданных
        // Далее выполняется поиск подробного описания объекта метаданных в коллекции _cache
        private ConcurrentDictionary<Guid, Dictionary<string, Guid>> _names;

        // Код типа объекта метаданных + тип объекта метаданных, например, "Справочник.Номенклатура"
        private Dictionary<int, Guid> _codes;

        // Идентификаторы объектов СУБД для объектов метаданных,
        // в том числе их реквизитов и вспомогательных таблиц СУБД
        private DbNameCache _data;

        #endregion

        internal InfoBaseCache(DatabaseProvider provider, in string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
        }

        internal string ConnectionString { get { return _connectionString; } }
        internal DatabaseProvider DatabaseProvider { get { return _provider; } }

        internal void Initialize(out InfoBase infoBase)
        {
            InitializeRootFile();
            InitializeMetadataCache(out infoBase);
            InitializeReferenceCache();
            InitializeDbNameCache();
        }
        private void InitializeRootFile()
        {
            using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Config, ConfigFiles.Root))
            {
                _root = new RootFileParser().Parse(in reader);
            }
        }
        private void InitializeMetadataCache(out InfoBase infoBase)
        {
            using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Config, _root))
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

            if (type == MetadataTypes.Constant)
            {
                return;
            }

            if (!(type == MetadataTypes.Catalog ||
                type == MetadataTypes.Document ||
                type == MetadataTypes.Enumeration ||
                type == MetadataTypes.Publication ||
                type == MetadataTypes.Characteristic ||
                type == MetadataTypes.SharedProperty ||
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
        private void InitializeDbNameCache()
        {
            using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Params, ConfigFiles.DbNames))
            {
                new DbNamesParser().Parse(in reader, out _data);
            }

            if (_data == null)
            {
                return;
            }

            _codes = new Dictionary<int, Guid>();

            foreach (DbName item in _data.DbNames)
            {
                if (item.Name == MetadataTokens.Chrc ||
                    item.Name == MetadataTokens.Enum ||
                    item.Name == MetadataTokens.Node ||
                    item.Name == MetadataTokens.Document ||
                    item.Name == MetadataTokens.Reference)
                {
                    _codes.Add(item.Code, item.Uuid);
                }
            }
        }

        internal int CountMetadataObjects(Guid type)
        {
            if (!_cache.TryGetValue(type, out Dictionary<Guid, WeakReference<MetadataObject>> entry))
            {
                return 0;
            }

            return entry.Count;
        }
        internal bool TryGetReferenceInfo(Guid reference, out ReferenceInfo info)
        {
            return _references.TryGetValue(reference, out info);
        }
        internal IEnumerable<MetadataObject> GetMetadataObjects(Guid type)
        {
            if (!_cache.TryGetValue(type, out Dictionary<Guid, WeakReference<MetadataObject>> entry))
            {
                yield break;
            }

            foreach (KeyValuePair<Guid, WeakReference<MetadataObject>> reference in entry)
            {
                if (!reference.Value.TryGetTarget(out MetadataObject metadata))
                {
                    UpdateMetadataObjectCache(type, reference.Key, out metadata);
                }
                yield return metadata;
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
            if (!_parsers.TryGetValue(type, out IMetadataObjectParser parser))
            {
                throw new InvalidOperationException($"Unsupported metadata type {{{type}}}");
            }
            else if (parser == null)
            {
                string metadataType = MetadataTypes.ResolveName(type);
                throw new InvalidOperationException($"Metadata type parser is under development \"{metadataType}\"");
            }

            if (type == MetadataTypes.SharedProperty)
            {
                GetSharedProperty(uuid, in parser, out metadata);
            }
            else if (type == MetadataTypes.NamedDataTypeSet)
            {
                GetNamedDataTypeSet(uuid, in parser, out metadata);
            }
            else
            {
                GetApplicationObject(uuid, in parser, out metadata);
            }
        }
        private void GetSharedProperty(Guid uuid, in IMetadataObjectParser parser, out MetadataObject metadata)
        {
            List<Guid> references;
            
            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata, out references);
            }

            SharedProperty target = metadata as SharedProperty;

            Configurator.ConfigureReferenceTypes(this, target.PropertyType, in references);
        }
        private void GetNamedDataTypeSet(Guid uuid, in IMetadataObjectParser parser, out MetadataObject metadata)
        {
            List<Guid> references;
            
            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata, out references);
            }

            NamedDataTypeSet target = metadata as NamedDataTypeSet;

            Configurator.ConfigureReferenceTypes(this, target.DataTypeSet, in references);
        }
        private void GetApplicationObject(Guid uuid, in IMetadataObjectParser parser, out MetadataObject metadata)
        {
            Dictionary<MetadataProperty, List<Guid>> references;

            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata, out references);
            }

            Configurator.ConfigureSystemProperties(this, in metadata);

            if (references != null && references.Count > 0)
            {
                Configurator.ConfigureMetadataProperties(this, in metadata, in references);
            }

            Configurator.ConfigureSharedProperties(this, in metadata);

            if (metadata is IPredefinedValues)
            {
                Configurator.ConfigurePredefinedValues(this, in metadata);
            }
        }
    }
}