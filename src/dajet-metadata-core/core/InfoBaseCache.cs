using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaJet.Metadata.Core
{
    public sealed class InfoBaseCache
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
        ///<br><b>Значение:</b> описание объекта метаданных и его кэшируемый экземпляр</br>
        ///</summary>
        private ConcurrentDictionary<Guid, Dictionary<Guid, MetadataEntry>> _cache;

        ///<summary>
        ///<br><b>Ключ:</b> UUID типа данных "Ссылка", например, "ОпределяемыйТип", "ЛюбаяСсылка",</br>
        ///<br>"СправочникСсылка", "СправочникСсылка.Номенклатура"  и т.п. (общие и конкретные типы данных).</br>
        ///<br></br>
        ///<br><b>Значение:</b> UUID объекта метаданных. Для ссылок на общие типы данных - значение <see cref="MetadataTypes"/>.</br>
        ///<br></br>
        ///<br><b>Использование:</b> расшифровка <see cref="DataTypeSet"/> при чтении файлов конфигурации.</br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, Guid> _references = new();

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
        ///<b>Коллекция документов и их регистров движения:</b>
        ///<br><b>Ключ:</b> UUID объекта метаданных <see cref="Document"/> - регистратора</br>
        ///<br><b>Значение:</b> список регистров движения <see cref="InformationRegister"/> или <see cref="AccumulationRegister"/></br>
        ///</summary>
        private readonly ConcurrentDictionary<Guid, List<Guid>> _registers = new();

        public void AddReference(Guid reference, Guid metadata)
        {
            _ = _references.TryAdd(reference, metadata);
        }
        public void AddCharacteristic(Guid characteristic, Guid metadata)
        {
            _ = _characteristics.TryAdd(characteristic, metadata);
        }
        public void AddCatalogOwner(Guid catalog, Guid owner)
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
        public void AddDocumentRegister(Guid document, Guid register)
        {
            if (_registers.TryGetValue(document, out List<Guid> registers))
            {
                registers.Add(register);
            }
            else
            {
                _registers.TryAdd(document, new List<Guid>() { register });
            }
        }

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

            _parsers = new()
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
            
            _references.Clear();
            _references.TryAdd(ReferenceTypes.AnyReference, Guid.Empty);
            _references.TryAdd(ReferenceTypes.Catalog, MetadataTypes.Catalog);
            _references.TryAdd(ReferenceTypes.Document, MetadataTypes.Document);
            _references.TryAdd(ReferenceTypes.Enumeration, MetadataTypes.Enumeration);
            _references.TryAdd(ReferenceTypes.Publication, MetadataTypes.Publication);
            _references.TryAdd(ReferenceTypes.Characteristic, MetadataTypes.Characteristic);

            _characteristics.Clear();

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(_cache, options, InitializeReferenceTypeCache);
        }
        private void InitializeReferenceTypeCache(KeyValuePair<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> cache)
        {
            Guid type = cache.Key;

            //TODO: get parser by metadata type !!!

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
                    MetadataEntry info = parser.Parse(in reader, type, out string name);

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
                    if (info.ReferenceUuid != Guid.Empty)
                    {
                        if (info.MetadataType == MetadataTypes.Characteristic)
                        {
                            _ = _references.TryAdd(info.CharacteristicUuid, info);
                        }
                        _ = _references.TryAdd(info.ReferenceUuid, info);
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
        internal bool TryGetReferenceInfo(Guid reference, out MetadataEntry info)
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
            //TODO: store reference types in DataTypeSet !?
            Dictionary<MetadataProperty, List<Guid>> references;

            //TODO: create metadata instance here and initialize system properties first ?
            // otherwise system properties are added to the end - not convinient from the JDTO serialization point of view ...
            // another way: inserting system properties into collection is not good for performance ...

            using (ConfigFileReader reader = new(_provider, _connectionString, ConfigTables.Config, uuid))
            {
                parser.Parse(in reader, out metadata, out references);
            }

            // Shared properties are always in the bottom.
            // They have default property purpose - Property.
            Configurator.ConfigureSharedProperties(this, in metadata);

            Configurator.ConfigureSystemProperties(this, in metadata);

            if (references != null && references.Count > 0)
            {
                // TODO: remove this call - store reference types in DataTypeSet !!!
                Configurator.ConfigureMetadataProperties(this, in metadata, in references);
            }

            if (metadata is ApplicationObject owner && metadata is IAggregate)
            {
                Configurator.ConfigureTableParts(this, in owner);
            }

            //TODO: configure database field names - lookup DbNameCache (_data)

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

        internal DbName GetDbName(Guid uuid)
        {
            if (!_data.TryGet(uuid, out DbName entry))
            {
                throw new InvalidOperationException(nameof(GetDbName));
            }

            return entry;
        }
        internal DbName GetLineNo(Guid uuid)
        {
            if (!_data.TryGet(uuid, out DbName entry))
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
            if (!_data.TryGet(uuid, out DbName entry))
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
    }
}