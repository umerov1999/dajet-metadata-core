﻿using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class MetadataPropertyCollectionParser
    {
        private readonly MetadataCache _cache;
        private ConfigFileParser _parser;
        private DataTypeSetParser _typeParser;
        private readonly MetadataObject _owner;

        private int _count; // количество свойств
        private PropertyPurpose _purpose;
        private MetadataProperty _property;
        private List<MetadataProperty> _target;
        private ConfigFileConverter _converter;
        public MetadataPropertyCollectionParser(MetadataCache cache)
        {
            _cache = cache;
        }
        public MetadataPropertyCollectionParser(MetadataCache cache, MetadataObject owner)
        {
            _cache = cache;
            _owner = owner;
        }
        public void Parse(in ConfigFileReader source, out List<MetadataProperty> target)
        {
            ConfigureCollectionConverter(in source);

            _target = new List<MetadataProperty>();

            _typeParser = new DataTypeSetParser(_cache);

            _parser = new ConfigFileParser();
            _parser.Parse(in source, in _converter);

            // result
            target = _target;

            // dispose private variables
            _target = null;
            _parser = null;
            _property = null;
            _converter = null;
            _typeParser = null;
        }
        
        private void ConfigureCollectionConverter(in ConfigFileReader source)
        {
            _converter = new ConfigFileConverter();

            // Параметр source должен быть позиционирован в данный момент
            // на узле коллекции свойств объекта метаданных (токен = '{')
            // source.Char == '{' && source.Token == TokenType.StartObject
            _converter = _converter.Path(source.Level - 1, source.Path);

            // Необходимо прекратить чтение коллекции,
            // чтобы позволить другим парсерам выполнить свою работу
            // по чтению потока байт source (данный парсер является вложенным)
            _converter += Cancel;

            // Свойства типизированной коллекции
            _converter[0] += Uuid; // идентификатор типа коллекции
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
            // тип коллекции свойств
            Guid type = source.GetUuid();

            if (type == SystemUuid.InformationRegister_Measure)
            {
                _purpose = PropertyPurpose.Measure;
            }
            else if (type == SystemUuid.InformationRegister_Dimension)
            {
                _purpose = PropertyPurpose.Dimension;
            }
            else
            {
                _purpose = PropertyPurpose.Property;
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

                _property = new MetadataProperty()
                {
                    Purpose = _purpose
                };

                _converter[0][1][1][1][1][2] += PropertyUuid;
                _converter[0][1][1][1][2] += PropertyName;
                _converter[0][1][1][1][3][2] += PropertyAlias;
                _converter[0][1][1][2] += PropertyType;

                if (_owner is not null && (_owner is Catalog || _owner is Characteristic))
                {
                    _converter[0][3] += PropertyUsage;
                }
            }

            // завершение чтения объекта свойства
            if (source.Token == TokenType.EndObject)
            {
                _target.Add(_property);
            }
        }
        private void PropertyUuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.Uuid = source.GetUuid();
        }
        private void PropertyName(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.Name = source.Value;
        }
        private void PropertyAlias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.Alias = source.Value;
        }
        private void PropertyType(in ConfigFileReader source, in CancelEventArgs args)
        {
            // Корневой узел объекта "ОписаниеТипов"

            if (source.Token == TokenType.StartObject)
            {
                _typeParser.Parse(in source, out DataTypeSet type);

                _property.PropertyType = type;
            }
        }
        private void PropertyUsage(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.PropertyUsage = (PropertyUsage)source.GetInt32();
        }
    }
}