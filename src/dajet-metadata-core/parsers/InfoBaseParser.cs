using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class InfoBaseParser
    {
        private readonly ConfigFileParser _parser = new();

        private ConfigFileConverter _converter;
        private readonly ConfigFileTokenHandler _cacheHandler;
        private readonly ConfigFileTokenHandler _metadataHandler;

        InfoBase _infoBase;
        private Dictionary<Guid, List<Guid>> _metadata;
        private ConcurrentDictionary<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> _cache;
        public InfoBaseParser()
        {
            _cacheHandler = new ConfigFileTokenHandler(MetadataCache);
            _metadataHandler = new ConfigFileTokenHandler(MetadataCollection);
        }
        private void ConfigureInfoBaseConverter()
        {
            // DONE: take file name from reader
            //_converter[1][0] += FileName; // Значение поля FileName в таблице Config

            // Свойства конфигурации
            _converter[3][1][1][1][1][2] += Name; // Наименование конфигурации
            _converter[3][1][1][1][1][3][2] += Alias; // Синоним
            _converter[3][1][1][1][1][4] += Comment; // Комментарий
            _converter[3][1][1][15] += ConfigVersion; // Версия конфигурации
            _converter[3][1][1][26] += Version; // Режим совместимости
            _converter[3][1][1][41] += SyncCallsMode; // Режим использования синхронных вызовов расширений платформы и внешних компонент
            _converter[3][1][1][36] += ModalWindowMode; // Режим использования модальности
            _converter[3][1][1][17] += DataLockingMode; // Режим управления блокировкой данных в транзакции по умолчанию
            _converter[3][1][1][19] += AutoNumberingMode; // Режим автонумерации объектов
            _converter[3][1][1][38] += UICompatibilityMode; // Режим совместимости интерфейса
        }
        private void ConfigureMetadataConverter()
        {
            // Коллекция объектов метаданных
            _converter[2] += ConfigureMetadataDictionary;
        }
        private void ConfigureCancellation()
        {
            // Прервать чтение файла после прочтения свойств конфигурации
            _converter[3][1][1] += Cancel;
        }
        private void InitializeMetadataCache()
        {
            _cache = new ConcurrentDictionary<Guid, Dictionary<Guid, WeakReference<MetadataObject>>>();
            _ = _cache.TryAdd(MetadataTypes.SharedProperty,       new Dictionary<Guid, WeakReference<MetadataObject>>()); // Общие реквизиты
            _ = _cache.TryAdd(MetadataTypes.NamedDataTypeSet,     new Dictionary<Guid, WeakReference<MetadataObject>>()); // Определяемые типы
            _ = _cache.TryAdd(MetadataTypes.Catalog,              new Dictionary<Guid, WeakReference<MetadataObject>>()); // Справочники
            _ = _cache.TryAdd(MetadataTypes.Document,             new Dictionary<Guid, WeakReference<MetadataObject>>()); // Документы
            _ = _cache.TryAdd(MetadataTypes.Constant,             new Dictionary<Guid, WeakReference<MetadataObject>>()); // Константы
            _ = _cache.TryAdd(MetadataTypes.Enumeration,          new Dictionary<Guid, WeakReference<MetadataObject>>()); // Перечисления
            _ = _cache.TryAdd(MetadataTypes.Publication,          new Dictionary<Guid, WeakReference<MetadataObject>>()); // Планы обмена
            _ = _cache.TryAdd(MetadataTypes.Characteristic,       new Dictionary<Guid, WeakReference<MetadataObject>>()); // Планы видов характеристик
            _ = _cache.TryAdd(MetadataTypes.InformationRegister,  new Dictionary<Guid, WeakReference<MetadataObject>>()); // Регистры сведений
            _ = _cache.TryAdd(MetadataTypes.AccumulationRegister, new Dictionary<Guid, WeakReference<MetadataObject>>()); // Регистры накопления
        }
        private void InitializeMetadataDictionary()
        {
            _metadata = new Dictionary<Guid, List<Guid>>()
            {
                { MetadataTypes.Subsystem,            new List<Guid>() }, // Подсистемы
                { MetadataTypes.NamedDataTypeSet,     new List<Guid>() }, // Определяемые типы
                { MetadataTypes.SharedProperty,       new List<Guid>() }, // Общие реквизиты
                { MetadataTypes.Catalog,              new List<Guid>() }, // Справочники
                { MetadataTypes.Constant,             new List<Guid>() }, // Константы
                { MetadataTypes.Document,             new List<Guid>() }, // Документы
                { MetadataTypes.Enumeration,          new List<Guid>() }, // Перечисления
                { MetadataTypes.Publication,          new List<Guid>() }, // Планы обмена
                { MetadataTypes.Characteristic,       new List<Guid>() }, // Планы видов характеристик
                { MetadataTypes.InformationRegister,  new List<Guid>() }, // Регистры сведений
                { MetadataTypes.AccumulationRegister, new List<Guid>() }  // Регистры накопления
            };
        }

        public void Parse(in ConfigFileReader reader, out InfoBase infoBase)
        {
            _converter = new ConfigFileConverter();

            ConfigureInfoBaseConverter();
            ConfigureCancellation();

            _infoBase = new InfoBase()
            {
                Uuid = new Guid(reader.FileName),
                YearOffset = reader.YearOffset,
                PlatformVersion = reader.PlatformVersion
            };

            _parser.Parse(in reader, in _converter);

            // Parsing result
            infoBase = _infoBase;

            // Dispose private variables
            _infoBase = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader reader, out Dictionary<Guid, List<Guid>> metadata)
        {
            _converter = new ConfigFileConverter();

            ConfigureMetadataConverter();

            InitializeMetadataDictionary();

            _parser.Parse(in reader, in _converter);

            // Parsing results
            metadata = _metadata;

            // Dispose private variables
            _metadata = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader reader, out InfoBase infoBase, out Dictionary<Guid, List<Guid>> metadata)
        {
            _converter = new ConfigFileConverter();

            ConfigureInfoBaseConverter();
            ConfigureMetadataConverter();

            _infoBase = new InfoBase()
            {
                Uuid = new Guid(reader.FileName),
                YearOffset = reader.YearOffset,
                PlatformVersion = reader.PlatformVersion
            };

            InitializeMetadataDictionary();

            _parser.Parse(in reader, in _converter);

            // Parsing results
            infoBase = _infoBase;
            metadata = _metadata;

            // Dispose private variables
            _infoBase = null;
            _metadata = null;
            _converter = null;
        }
        public void Parse(in ConfigFileReader reader, out InfoBase infoBase, out ConcurrentDictionary<Guid, Dictionary<Guid, WeakReference<MetadataObject>>> cache)
        {
            _converter = new ConfigFileConverter();

            ConfigureInfoBaseConverter();
            ConfigureMetadataConverter();

            _infoBase = new InfoBase()
            {
                Uuid = new Guid(reader.FileName),
                YearOffset = reader.YearOffset,
                PlatformVersion = reader.PlatformVersion
            };

            InitializeMetadataCache();

            _parser.Parse(in reader, in _converter);

            // Parsing results
            cache = _cache;
            infoBase = _infoBase;

            // Dispose private variables
            _cache = null;
            _infoBase = null;
            _converter = null;
        }

        #region "Свойства конфигурации"

        private void FileName(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.Uuid = source.GetUuid();
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.Name = source.Value;
        }
        private void Alias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.Alias = source.Value;
        }
        private void Comment(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.Comment = source.Value;
        }
        private void Version(in ConfigFileReader source, in CancelEventArgs args)
        {
            int version = source.GetInt32();

            if (version == 0)
            {
                _infoBase.СompatibilityVersion = 80216;
            }
            else if (version == 1)
            {
                _infoBase.СompatibilityVersion = 80100;
            }
            else if (version == 2)
            {
                _infoBase.СompatibilityVersion = 80213;
            }
            else
            {
                _infoBase.СompatibilityVersion = version;
            }
        }
        private void ConfigVersion(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.AppConfigVersion = source.Value;
        }
        private void SyncCallsMode(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.SyncCallsMode = (SyncCallsMode)source.GetInt32();
        }
        private void ModalWindowMode(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.ModalWindowMode = (ModalWindowMode)source.GetInt32();
        }
        private void DataLockingMode(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.DataLockingMode = (DataLockingMode)source.GetInt32();
        }
        private void AutoNumberingMode(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.AutoNumberingMode = (AutoNumberingMode)source.GetInt32();
        }
        private void UICompatibilityMode(in ConfigFileReader source, in CancelEventArgs args)
        {
            _infoBase.UICompatibilityMode = (UICompatibilityMode)source.GetInt32();
        }

        #endregion

        #region "Коллекция объектов метаданных"

        private void ConfigureMetadataDictionary(in ConfigFileReader source, in CancelEventArgs args)
        {
            int offset = 2; // текущая позиция [2] - последующие позиции +1
            int count = source.GetInt32(); // количество компонентов платформы

            while (count > 0)
            {
                _converter[offset + count][0] += ConfigureComponent;
                count--;
            }
        }
        private void ConfigureComponent(in ConfigFileReader source, in CancelEventArgs args)
        {
            Guid uuid = source.GetUuid(); // Идентификатор компоненты платформы

            // Родительский узел компоненты платформы
            // NOTE: Последовательность узлов компонентов платформы может быть не гарантирована.
            int node = source.Path[0];

            if (uuid == Components.General) // 3.0 - Компонента платформы "Общие объекты"
            {
                _converter[node][1][2] += ConfigureMetadata; // начало коллекции объектов метаданных компоненты
            }
            else if (uuid == Components.Operations) // 4.0 - Компонента платформы "Оперативный учёт"
            {
                _converter[node][1][1][2] += ConfigureMetadata; // начало коллекции объектов метаданных компоненты
            }   
        }
        private void ConfigureMetadata(in ConfigFileReader source, in CancelEventArgs args)
        {
            int count = source.GetInt32(); // количество объектов метаданных компоненты
            int offset = source.Path[source.Level]; // 3.1.2 текущая позиция - последующие позиции +1
            
            ConfigFileConverter node = _converter.Path(source.Level - 1, source.Path); // родительский узел компоненты
            
            while (count > 0)
            {
                node[offset + count] += (_cache != null) ? _cacheHandler : _metadataHandler;
                count--;
            }
        }
        private void MetadataCache(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                return;
            }

            int count = 0;
            Guid type = Guid.Empty;

            if (source.Read()) // 0 - Идентификатор типа объекта метаданных
            {
                type = source.GetUuid();
            }

            if (!_cache.TryGetValue(type, out Dictionary<Guid, WeakReference<MetadataObject>> collection))
            {
                return; // Неподдерживаемый или неизвестный тип объекта метаданных
            }

            if (source.Read()) // 1 - Количество объектов метаданных в коллекции
            {
                count = source.GetInt32();
            }

            while (count > 0)
            {
                if (!source.Read())
                {
                    break;
                }

                Guid uuid = source.GetUuid(); // Идентификатор объекта метаданных

                if (uuid != Guid.Empty)
                {
                    collection.Add(uuid, new WeakReference<MetadataObject>(null));
                }

                count--;
            }
        }
        private void MetadataCollection(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                return;
            }

            int count = 0;
            Guid type = Guid.Empty;

            if (source.Read()) // 0 - Идентификатор типа объекта метаданных
            {
                type = source.GetUuid();
            }

            if (!_metadata.TryGetValue(type, out List<Guid> collection))
            {
                return; // Неподдерживаемый или неизвестный тип объекта метаданных
            }

            if (source.Read()) // 1 - Количество объектов метаданных в коллекции
            {
                count = source.GetInt32();
            }

            while (count > 0)
            {
                if (!source.Read())
                {
                    break;
                }

                Guid uuid = source.GetUuid(); // Идентификатор объекта метаданных

                if (uuid != Guid.Empty)
                {
                    collection.Add(uuid);
                }

                count--;
            }
        }

        #endregion

        private void Cancel(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                args.Cancel = true;
            }
        }
    }
}