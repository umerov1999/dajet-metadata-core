using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class MetadataPropertyCollectionParser
    {
        private ConfigFileParser _parser;

        private Guid _type; // тип коллекции свойств
        private int _count; // количество свойств
        private MetadataProperty _item;
        private PropertyPurpose _purpose;
        private List<MetadataProperty> _target;
        private ConfigFileConverter _converter;
        public void Parse(in ConfigFileReader source, out List<MetadataProperty> target)
        {
            ConfigureCollectionConverter(in source);

            _target = new List<MetadataProperty>();

            _parser = new ConfigFileParser();
            _parser.Parse(in source, in _converter);

            target = _target; // result

            // dispose private variables
            _target = null;
            _parser = null;
            _converter = null;
        }
        
        private void ConfigureCollectionConverter(in ConfigFileReader source)
        {
            _converter = new ConfigFileConverter();

            // Параметр source должен быть позиционирован в данный момент
            // на узле коллекции свойств объекта метаданных (токен = '{')
            // source.Char == '{' && source.Token == TokenType.StartObject
            _converter = _converter.Path(source.Level - 1, source.Path);

            // Необходимо прекратить чтение коллекции,
            // чтобы позволить другим парсерам выполнить свою работу,
            // по чтению потока байт source (данный парсер является вложенным)
            _converter += Cancel;

            // Свойства типизированной коллекции
            _converter[0] += Uuid; // идентификатор (параметр типа коллекции - тип данных элементов коллекции)
            _converter[1] += Count; // количество элементов в коллекции

            // Объекты элементов коллекции, в зависимости от значения _converter[1],
            // располагаются в коллекции последовательно по адресам _converter[2..N]
        }
        private void Cancel(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                args.Cancel = true;
            }
        }
        private void Uuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            _type = source.GetUuid();
            ConfigurePropertyPurpose();
        }
        private void ConfigurePropertyPurpose()
        {
            if (_type == MetadataRegistry.InformationRegister_Measure)
            {
                _purpose = PropertyPurpose.Measure;
            }
            else if (_type == MetadataRegistry.InformationRegister_Property)
            {
                _purpose = PropertyPurpose.Property;
            }
            else if (_type == MetadataRegistry.InformationRegister_Dimension)
            {
                _purpose = PropertyPurpose.Dimension;
            }
        }
        private void Count(in ConfigFileReader source, in CancelEventArgs args)
        {
            _count = source.GetInt32();
            
            ConfigureItemConverters();
        }
        
        private void ConfigureItemConverters()
        {
            int offset = 2; // начальный индекс для узлов элементов коллекции от её корня

            for (int n = 0; n < _count; n++)
            {
                ConfigureItemConverter(offset + n);
            }
        }
        private void ConfigureItemConverter(int offset)
        {
            _converter[offset] += MetadataPropertyConverter;
        }
        private void MetadataPropertyConverter(in ConfigFileReader source, in CancelEventArgs args)
        {
            // начало чтения объекта свойства
            if (source.Token == TokenType.StartObject)
            {
                // корневой узел объекта свойства
                _converter = _converter.Path(source.Level - 1, source.Path);

                _item = new MetadataProperty()
                {
                    Purpose = _purpose
                };

                // Если это иерархический тип объекта метаданных (Справочник или ПланВидовХарактеристик),
                // тогда читаем настройку использования свойства для групп или элементов
                // TODO: _converter[0][3] += PropertyUsage;

                _converter[0][1][1][1][1][2] += PropertyUuid;
                _converter[0][1][1][1][2] += PropertyName;
                _converter[0][1][1][1][3][2] += PropertyAlias;
                _converter[0][1][1][2] += PropertyType;
            }

            // завершение чтения объекта свойства
            if (source.Token == TokenType.EndObject)
            {
                MetadataProperty property = _item;
                _target.Add(property);
                _item = null;
            }
        }
        private void PropertyUuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            _item.Uuid = source.GetUuid();
        }
        private void PropertyName(in ConfigFileReader source, in CancelEventArgs args)
        {
            _item.Name = source.Value;
        }
        private void PropertyAlias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _item.Alias = source.Value;
        }
        private void PropertyType(in ConfigFileReader source, in CancelEventArgs args)
        {
            // Описание типа данных свойства (корневой узел объекта DataTypeSet)

            if (source.Token == TokenType.StartObject)
            {
                // FIXME: DataTypeSetParser.Parse(in source, out DataTypeSet type);

                //_item.PropertyType = type;
            }
        }

        //        int propertiesCount = properties.GetInt32(new int[] { 1 }); // количество реквизитов
        //        if (propertiesCount == 0) return;

        //        int propertyOffset = 2;
        //        for (int p = 0; p < propertiesCount; p++)
        //        {
        //            // P.0.1.1.1.1.2 - property uuid
        //            Guid propertyUuid = properties.GetUuid(new int[] { p + propertyOffset, 0, 1, 1, 1, 1, 2 });
        //            // P.0.1.1.1.2 - property name
        //            string propertyName = properties.GetString(new int[] { p + propertyOffset, 0, 1, 1, 1, 2 });
        //            // P.0.1.1.1.3 - property alias descriptor
        //            string propertyAlias = string.Empty;
        //            ConfigObject aliasDescriptor = properties.GetObject(new int[] { p + propertyOffset, 0, 1, 1, 1, 3 });
        //            if (aliasDescriptor.Values.Count == 3)
        //            {
        //                // P.0.1.1.1.3.2 - property alias
        //                propertyAlias = properties.GetString(new int[] { p + propertyOffset, 0, 1, 1, 1, 3, 2 });
        //            }
        //            // P.0.1.1.2 - property types
        //            ConfigObject propertyTypes = properties.GetObject(new int[] { p + propertyOffset, 0, 1, 1, 2 });
        //            // P.0.1.1.2.0 = "Pattern"

        //            // TODO !!! DataTypeSet typeInfo = (DataTypeInfo)GetConverter<DataTypeInfo>().Convert(propertyTypes);
        //            DataTypeSet typeInfo = new DataTypeSet();

        //            // P.0.3 - property usage for catalogs and characteristics
        //            int propertyUsage = -1;
        //            if (metaObject is Catalog || metaObject is Characteristic)
        //            {
        //                propertyUsage = properties.GetInt32(new int[] { p + propertyOffset, 0, 3 });
        //            }

        //            ConfigureProperty(metaObject, purpose, propertyUuid, propertyName, propertyAlias, typeInfo, propertyUsage);
        //        }
    }
}