using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class InfoBaseParser : IMetadataObjectParser
    {
        private readonly ConfigFileParser _parser = new ConfigFileParser();

        private ConfigFileConverter _converter;

        InfoBase _infoBase;
        private Dictionary<Guid, List<Guid>> _collections;
        private void ConfigureConfigFileConverter()
        {
            _converter = new ConfigFileConverter();

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

            // Коллекции объектов метаданных
            _converter[2] += ConfigureMetadataCollections;
        }
        private void InitializeMetadataCollectionsLookup()
        {
            _collections = new Dictionary<Guid, List<Guid>>();
            _collections.Add(MetadataRegistry.Subsystems, new List<Guid>()); // Подсистемы
            _collections.Add(MetadataRegistry.NamedDataTypeSets, new List<Guid>()); // Определяемые типы
            _collections.Add(MetadataRegistry.SharedProperties, new List<Guid>()); // Общие реквизиты
            _collections.Add(MetadataRegistry.Catalogs, new List<Guid>());
            _collections.Add(MetadataRegistry.Constants, new List<Guid>());
            _collections.Add(MetadataRegistry.Documents, new List<Guid>());
            _collections.Add(MetadataRegistry.Enumerations, new List<Guid>());
            _collections.Add(MetadataRegistry.Publications, new List<Guid>()); // Планы обмена
            _collections.Add(MetadataRegistry.Characteristics, new List<Guid>());
            _collections.Add(MetadataRegistry.InformationRegisters, new List<Guid>());
            _collections.Add(MetadataRegistry.AccumulationRegisters, new List<Guid>());
        }
        public void Parse(in ConfigFileReader reader, out InfoBase infoBase, out Dictionary<Guid, List<Guid>> collections)
        {
            ConfigureConfigFileConverter();

            _infoBase = new InfoBase()
            {
                Uuid = new Guid(reader.FileName),
                YearOffset = reader.YearOffset,
                PlatformVersion = reader.PlatformVersion
            };
            InitializeMetadataCollectionsLookup();

            infoBase = _infoBase;
            collections = _collections;

            _parser.Parse(in reader, in _converter);

            // TODO: Dispose()
            _infoBase = null;
            _collections = null; // Do not call _collections.Clear() here =) This will clear collections parameter also =)
            _converter = null;
        }

        public void Parse(in ConfigFileReader source, out MetadataObject target)
        {
            throw new NotImplementedException();
        }
        public void Parse(in ConfigFileReader source, in string name, out MetadataObject target)
        {
            throw new NotImplementedException();
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

        #region "Коллекции объектов метаданных"

        private void ConfigureMetadataCollections(in ConfigFileReader source, in CancelEventArgs args)
        {
            int offset = 2; // текущая позиция - последующие позиции +1
            int count = source.GetInt32();
            while (count > 0)
            {
                _converter[offset + count][0] += ConfigureMetadataCollection;
                count--;
            }
        }
        private void ConfigureMetadataCollection(in ConfigFileReader source, in CancelEventArgs args)
        {
            Guid uuid = source.GetUuid();

            if (uuid == Guid.Empty)
            {
                return;
            }
            
            try
            {
                int node = source.Path[0]; // последовательность описания подсистем платформы не гарантирована !

                if (uuid == MetadataRegistry.Subsystem_Common) // 3.0 - Подсистема общих объектов
                {
                    _converter[node][1][2] += ConfigureReadingTypedCollections; // начало типизированного списка
                }
                else if (uuid == MetadataRegistry.Subsystem_Operations) // 4.0 - Подсистема прикладных объектов
                {
                    _converter[node][1][1][2] += ConfigureReadingTypedCollections; // начало типизированного списка
                }
            }
            catch
            {
                // do nothing
            }       
        }
        private void ConfigureReadingTypedCollections(in ConfigFileReader source, in CancelEventArgs args)
        {
            int count = source.GetInt32(); // количество элементов типизированной коллекции
            int offset = source.Path[source.Level]; // 3.1.2 текущая позиция - последующие позиции +1
            ConfigFileConverter node = _converter.Path(source.Level - 1, source.Path); // родительский узел коллекции
            while (count > 0)
            {
                node[offset + count] += ReadTypedCollection;
                count--;
            }
        }
        private void ReadTypedCollection(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                return;
            }

            int count = 0; // 1 - number of items in the collection
            Guid type = Guid.Empty; // 0 - type uuid of the collection

            if (source.Read()) { type = source.GetUuid(); }
            if (type == Guid.Empty) { return; }

            if (!_collections.TryGetValue(type, out List<Guid> collection))
            {
                return;
            }

            if (source.Read()) { count = source.GetInt32(); }
            if (count == 0 || count == -1) { return; }

            for (int i = 0; i < count; i++)
            {
                if (source.Read())
                {
                    Guid uuid = source.GetUuid();

                    if (uuid == Guid.Empty) { continue; }

                    if (_name != null)
                    {
                        if (ParseApplicationObjectByName(in source, type, uuid))
                        {
                            //collection.Add(uuid);
                            args.Cancel = true;
                            return;
                        }
                    }
                    else if (_uuid != Guid.Empty && _uuid == uuid)
                    {
                        ParseApplicationObject(in source, type);
                        //collection.Add(uuid);
                        args.Cancel = true;
                        return;
                    }
                    else // no filter
                    {
                        collection.Add(uuid);
                    }
                }
            }
        }
        private bool ParseApplicationObjectByName(in ConfigFileReader source, Guid type, Guid uuid)
        {
            if (_name == null)
            {
                return false;
            }

            if (!MetadataParserFactory.TryGetParser(type, out IMetadataObjectParser parser))
            {
                return false;
            }

            using (ConfigFileReader reader = new ConfigFileReader(source.DatabaseProvider, source.ConnectionString, ConfigTableNames.Config, uuid))
            {
                parser.Parse(in reader, in _name, out _target);
            }

            return (_target != null);
        }
        private void ParseApplicationObject(in ConfigFileReader source, Guid type)
        {
            if (_uuid == Guid.Empty)
            {
                return;
            }

            if (!MetadataParserFactory.TryGetParser(type, out IMetadataObjectParser parser))
            {
                return;
            }

            using (ConfigFileReader reader = new ConfigFileReader(source.DatabaseProvider, source.ConnectionString, ConfigTableNames.Config, _uuid))
            {
                parser.Parse(in reader, out _target);
            }
        }

        #endregion

        #region "Поиск и загрузка объекта метаданных по его имени или уникальному идентификатору"
        
        private Guid _uuid;
        private string _name;
        private MetadataObject _target;
        public void ParseByName(in ConfigFileReader reader, Guid type, in string name, out MetadataObject target)
        {
            _converter = new ConfigFileConverter();
            _converter[2] += ConfigureMetadataCollections;

            // filters
            _name = name;
            _collections = new Dictionary<Guid, List<Guid>>() { { type, new List<Guid>() } };

            // execute parser
            _parser.Parse(in reader, in _converter);

            target = _target; // result

            // TODO: Dispose()
            _name = null;
            _target = null;
            _converter = null;
            _collections.Clear();
            _collections = null;
        }
        public void ParseByUuid(in ConfigFileReader reader, Guid type, Guid uuid, out MetadataObject target)
        {
            _converter = new ConfigFileConverter();
            _converter[2] += ConfigureMetadataCollections;

            // filters
            _uuid = uuid;
            _collections = new Dictionary<Guid, List<Guid>>() { { type, new List<Guid>() } };

            // execute parser
            _parser.Parse(in reader, in _converter);

            target = _target; // result

            // TODO: Dispose()
            _uuid = Guid.Empty;
            _target = null;
            _converter = null;
            _collections.Clear();
            _collections = null;
        }

        #endregion

        private void ConfigureCompoundTypes(in ConfigObject config, in InfoBase infoBase)
        {
            //// количество объектов в коллекции
            //int count = config.GetInt32(1);
            //if (count == 0) return;

            //// 3.1.23.N - идентификаторы файлов определяемых типов
            //int offset = 2;
            //NamedDataTypeSet compound;
            //for (int i = 0; i < count; i++)
            //{
            //    compound = new NamedDataTypeSet()
            //    {
            //        FileName = config.GetUuid(i + offset)
            //    };
            //    ConfigureCompoundType(in compound, in infoBase);
            //    infoBase.CompoundTypes.Add(compound.Uuid, compound);
            //}
        }
        private void ConfigureCompoundType(in NamedDataTypeSet compound, in InfoBase infoBase)
        {
            //ConfigObject config = ConfigFileParser.Parse(ConfigTableNames.Config, compound.FileName.ToString());

            //compound.Uuid = config[1].GetUuid(1);
            //compound.Name = config[1][3].GetString(2);
            //ConfigObject alias = config[1][3][3];
            //if (alias.Count == 3)
            //{
            //    compound.Alias = config[1][3][3].GetString(2);
            //}
            //// 1.3.4 - комментарий
            //// TODO: add Comment property to MetadataObject ?

            //// 1.4 - описание типов значений определяемого типа
            //ConfigObject types = config[1][4];
            //// TODO: compound.TypeInfo = (DataTypeInfo)TypeInfoConverter.Convert(types);
        }
    }
}