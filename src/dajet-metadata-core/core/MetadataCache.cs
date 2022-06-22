using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaJet.Metadata.Core
{
    public sealed class MetadataCache
    {
        private readonly string _connectionString;
        private readonly DatabaseProvider _provider;
        private readonly Dictionary<Guid, IMetadataObjectParser> _parsers;

        #region "PRIVATE CACHE VALUES"

        ///<summary>Корневой файл конфигурации из файла "root" таблицы "Config"</summary>
        private Guid _root = Guid.Empty;

        ///<summary>
        ///<b>Кэш объектов метаданных:</b>
        ///<br><b>Ключ 1:</b> UUID общего типа метаданных, например, "Справочник"</br>
        ///<br><b>Ключ 2:</b> UUID объекта метаданных, например, "Справочник.Номенклатура"</br>
        ///<br><b>Значение:</b> кэшируемый объект метаданных</br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> _cache = new();

        ///<summary>
        ///<b>Имена объектов метаданных:</b>
        ///<br><b>Ключ 1:</b> UUID общего типа метаданных, например, "Справочник"</br>
        ///<br><b>Ключ 2:</b> имя объекта метаданных, например, "Номенклатура"</br>
        ///<br><b>Значение:</b> UUID объекта метаданных, например, "Справочник.Номенклатура"</br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, Dictionary<string, Guid>> _names = new();

        ///<summary>
        ///<br><b>Ключ:</b> UUID типа данных "Ссылка", например, "ОпределяемыйТип", "ЛюбаяСсылка",</br>
        ///<br>"СправочникСсылка", "СправочникСсылка.Номенклатура"  и т.п. (общие и конкретные типы данных).</br>
        ///<br></br>
        ///<br><b>Значение:</b> UUID общего и конкретного типов объекта метаданных.</br>
        ///<br>Для ссылок на общие типы данных - значение <see cref="MetadataTypes"/>.</br>
        ///<br></br>
        ///<br><b>Использование:</b> расшифровка <see cref="DataTypeSet"/> при чтении файлов конфигурации.</br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, MetadataEntry> _references = new();

        ///<summary>
        ///<br><b>Ключ:</b> UUID типа данных "Характеристика" - исключительный случай для <see cref="_references"/>,</br>
        ///<br>так как невозможно сопоставить одновременно общий тип, конкретный тип</br>
        ///<br>и тип "Характеристика" (для последнего нет UUID).</br>
        ///<br></br>
        ///<br><b>Значение:</b> UUID объекта метаданных типа "ПланВидовХарактеристик".</br>
        ///<br></br>
        ///<br><b>Использование:</b> расшифровка <see cref="DataTypeSet"/> при чтении файлов конфигурации.</br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, Guid> _characteristics = new();

        ///<summary>
        ///<b>Коллекция подчинённых справочников и их владельцев:</b>
        ///<br><b>Ключ:</b> UUID объекта метаданных <see cref="Catalog"/></br>
        ///<br><b>Значение:</b> список UUID объектов метаданных <see cref="Catalog"/> - владельцев справочника</br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, List<Guid>> _owners = new();

        ///<summary>
        ///<b>Коллекция документов и их регистров движений:</b>
        ///<br><b>Ключ:</b> регистр движений <see cref="InformationRegister"/> или <see cref="AccumulationRegister"/></br>
        ///<br><b>Значение:</b> список регистраторов движений <see cref="Document"/></br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, List<Guid>> _registers = new();

        ///<summary>
        ///<br>Кэш идентификаторов <see cref="DbName"/> объектов СУБД</br>
        ///<br>и их сопоставление объектам метаданных конфигурации</br>
        ///</summary>
        private DbNameCache _database;
        internal DbName GetDbName(Guid uuid)
        {
            if (!_database.TryGet(uuid, out DbName entry))
            {
                throw new InvalidOperationException(nameof(GetDbName));
            }

            return entry;
        }
        internal DbName GetLineNo(Guid uuid)
        {
            if (!_database.TryGet(uuid, out DbName entry))
            {
                throw new InvalidOperationException(nameof(GetDbName));
            }

            foreach (DbName child in entry.Children)
            {
                if (child.Name == MetadataTokens.LineNo)
                {
                    return child;
                }
            }

            throw new InvalidOperationException(nameof(GetLineNo));
        }
        internal DbName GetChngR(Guid uuid)
        {
            if (!_database.TryGet(uuid, out DbName entry))
            {
                throw new InvalidOperationException(nameof(GetDbName));
            }

            foreach (DbName child in entry.Children)
            {
                if (child.Name.EndsWith(MetadataTokens.ChngR))
                {
                    return child;
                }
            }

            throw new InvalidOperationException(nameof(GetChngR));
        }

        private void AddName(Guid type, Guid metadata, string name)
        {
            // metadata object name to uuid mapping
            if (_names.TryGetValue(type, out Dictionary<string, Guid> names))
            {
                names.Add(name, metadata);
            }
            else
            {
                _ = _names.TryAdd(type, new Dictionary<string, Guid>()
                {
                    { name, metadata }
                });
            }
        }
        private void AddReference(Guid reference, MetadataEntry metadata)
        {
            _ = _references.TryAdd(reference, metadata);
        }
        private void AddCharacteristic(Guid characteristic, Guid metadata)
        {
            _ = _characteristics.TryAdd(characteristic, metadata);
        }
        private void AddCatalogOwner(Guid catalog, Guid owner)
        {
            if (_owners.TryGetValue(catalog, out List<Guid> owners))
            {
                owners.Add(owner);
            }
            else
            {
                _owners.TryAdd(catalog, new List<Guid>() { owner });
            }
        }
        private void AddDocumentRegister(Guid document, Guid register)
        {
            if (_registers.TryGetValue(register, out List<Guid> documents))
            {
                documents.Add(document);
            }
            else
            {
                _registers.TryAdd(register, new List<Guid>() { document });
            }
        }

        internal List<Guid> GetCatalogOwners(Guid catalog)
        {
            if (_owners.TryGetValue(catalog, out List<Guid> owners))
            {
                return owners;
            }

            return null;
        }
        internal List<Guid> GetRegisterRecorders(Guid register)
        {
            if (_registers.TryGetValue(register, out List<Guid> documents))
            {
                return documents;
            }

            return null;
        }

        #endregion

        internal MetadataCache(DatabaseProvider provider, in string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;

            _parsers = new() // supported metadata object parsers
            {
                { MetadataTypes.Catalog, new CatalogParser(this) },
                { MetadataTypes.Document, new DocumentParser(this) },
                { MetadataTypes.Enumeration, new EnumerationParser(this) },
                { MetadataTypes.Publication, new PublicationParser(this) },
                { MetadataTypes.Characteristic, new CharacteristicParser(this) },
                { MetadataTypes.InformationRegister, new InformationRegisterParser(this) },
                { MetadataTypes.AccumulationRegister, new AccumulationRegisterParser(this) },
                { MetadataTypes.SharedProperty, new SharedPropertyParser(this) },
                { MetadataTypes.NamedDataTypeSet, new NamedDataTypeSetParser(this) } // since 1C:Enterprise 8.3.3 version
            };
        }
        internal string ConnectionString { get { return _connectionString; } }
        internal DatabaseProvider DatabaseProvider { get { return _provider; } }
        internal void Initialize(out InfoBase infoBase)
        {
            InitializeRootFile();
            InitializeMetadataCache(out infoBase);
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
            _cache.Clear();
            _names.Clear();
            _owners.Clear();
            _registers.Clear();
            _references.Clear();
            _characteristics.Clear();

            _references.TryAdd(ReferenceTypes.AnyReference, new MetadataEntry(Guid.Empty, Guid.Empty));
            _references.TryAdd(ReferenceTypes.Catalog, new MetadataEntry(MetadataTypes.Catalog, Guid.Empty));
            _references.TryAdd(ReferenceTypes.Document, new MetadataEntry(MetadataTypes.Document, Guid.Empty));
            _references.TryAdd(ReferenceTypes.Enumeration, new MetadataEntry(MetadataTypes.Enumeration, Guid.Empty));
            _references.TryAdd(ReferenceTypes.Publication, new MetadataEntry(MetadataTypes.Publication, Guid.Empty));
            _references.TryAdd(ReferenceTypes.Characteristic, new MetadataEntry(MetadataTypes.Characteristic, Guid.Empty));

            Dictionary<Guid, List<Guid>> metadata = new()
            {
                //{ MetadataTypes.Constant,             new List<Guid>() }, // Константы
                //{ MetadataTypes.Subsystem,            new List<Guid>() }, // Подсистемы
                { MetadataTypes.NamedDataTypeSet,     new List<Guid>() }, // Определяемые типы
                { MetadataTypes.SharedProperty,       new List<Guid>() }, // Общие реквизиты
                { MetadataTypes.Catalog,              new List<Guid>() }, // Справочники
                { MetadataTypes.Document,             new List<Guid>() }, // Документы
                { MetadataTypes.Enumeration,          new List<Guid>() }, // Перечисления
                { MetadataTypes.Publication,          new List<Guid>() }, // Планы обмена
                { MetadataTypes.Characteristic,       new List<Guid>() }, // Планы видов характеристик
                { MetadataTypes.InformationRegister,  new List<Guid>() }, // Регистры сведений
                { MetadataTypes.AccumulationRegister, new List<Guid>() }  // Регистры накопления
            };

            using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Config, _root))
            {
                new InfoBaseParser().Parse(in reader, out infoBase, in metadata);
            }

            foreach (var entry in metadata)
            {
                Dictionary<Guid, WeakReference<MetadataObject>> items = new();

                if (!_cache.TryAdd(entry.Key, items))
                {
                    continue;
                }

                if (entry.Value.Count == 0)
                {
                    continue;
                }

                foreach (Guid item in entry.Value)
                {
                    _ = items.TryAdd(item, new WeakReference<MetadataObject>(null));
                }
            }

            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            ParallelLoopResult result = Parallel.ForEach(_cache, options, InitializeMetadata);
        }
        private void InitializeMetadata(KeyValuePair<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> cache)
        {
            Guid type = cache.Key; // общий тип объектов метаданных, например, "Справочник"

            if (!_parsers.TryGetValue(type, out IMetadataObjectParser parser))
            {
                return; // Unsupported metadata type
            }

            foreach (var entry in cache.Value)
            {
                if (entry.Key == Guid.Empty)
                {
                    continue;
                }

                using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Config, entry.Key))
                {
                    parser.Parse(in reader, out MetadataInfo metadata);

                    if (string.IsNullOrWhiteSpace(metadata.Name))
                    {
                        continue; // accidentally: unsupported metadata object type
                    }

                    if (!string.IsNullOrWhiteSpace(metadata.Name))
                    {
                        AddName(metadata.MetadataType, metadata.MetadataUuid, metadata.Name);
                    }

                    if (metadata.ReferenceUuid != Guid.Empty)
                    {
                        AddReference(metadata.ReferenceUuid, new MetadataEntry(metadata.MetadataType, metadata.MetadataUuid));
                    }

                    if (metadata.CharacteristicUuid != Guid.Empty)
                    {
                        AddCharacteristic(metadata.CharacteristicUuid, metadata.MetadataUuid);
                    }

                    if (metadata.CatalogOwners.Count > 0)
                    {
                        foreach (Guid owner in metadata.CatalogOwners)
                        {
                            AddCatalogOwner(metadata.MetadataUuid, owner);
                        }
                    }

                    if (metadata.DocumentRegisters.Count > 0)
                    {
                        foreach (Guid register in metadata.DocumentRegisters)
                        {
                            AddDocumentRegister(metadata.MetadataUuid, register);
                        }
                    }
                }
            }
        }
        private void InitializeDbNameCache()
        {
            //TODO: add option to load or not DbNames !?

            using (ConfigFileReader reader = new(_provider, in _connectionString, ConfigTables.Params, ConfigFiles.DbNames))
            {
                new DbNamesParser().Parse(in reader, out _database);
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
        internal bool TryGetReferenceInfo(Guid reference, out MetadataEntry info)
        {
            return _references.TryGetValue(reference, out info);
        }
        internal bool IsCharacteristic(Guid reference)
        {
            return _characteristics.TryGetValue(reference, out _);
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

            if (type == MetadataTypes.SharedProperty || type == MetadataTypes.NamedDataTypeSet)
            {
                using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
                {
                    parser.Parse(in reader, out metadata);
                }

                if (type == MetadataTypes.SharedProperty)
                {
                    Configurator.ConfigureDatabaseNames(this, in metadata);
                }
            }
            else
            {
                GetApplicationObject(uuid, in parser, out metadata);
            }
        }
        private void GetApplicationObject(Guid uuid, in IMetadataObjectParser parser, out MetadataObject metadata)
        {
            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata);
            }

            // Shared properties are always in the bottom.
            // They have default property purpose - Property.
            Configurator.ConfigureSharedProperties(this, in metadata);

            Configurator.ConfigureSystemProperties(this, in metadata);

            if (metadata is ApplicationObject owner && metadata is IAggregate)
            {
                Configurator.ConfigureTableParts(this, in owner);
            }

            Configurator.ConfigureDatabaseNames(this, in metadata); //TODO: option to configure database names

            if (metadata is IPredefinedValues) //TODO: option to load predefined values
            {
                try
                {
                    Configurator.ConfigurePredefinedValues(this, in metadata);
                }
                catch (Exception error)
                {
                    if (error.Message == "Zero length file")
                    {
                        // Metadata object has no predefined values file in Config table
                    }
                }
            }
        }
    }
}