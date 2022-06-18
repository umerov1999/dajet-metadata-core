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
        private DataTypeSetParser _typeParser;
        private readonly MetadataObject _owner;

        private int _count; // количество свойств
        private PropertyPurpose _purpose;
        private MetadataProperty _property;
        private List<MetadataProperty> _target;
        private Dictionary<MetadataProperty, List<Guid>> _references;
        private ConfigFileConverter _converter;
        public MetadataPropertyCollectionParser() { }
        public MetadataPropertyCollectionParser(MetadataObject owner)
        {
            _owner = owner;
        }
        public void Parse(in ConfigFileReader source, out List<MetadataProperty> target, out Dictionary<MetadataProperty, List<Guid>> references)
        {
            ConfigureCollectionConverter(in source);

            _target = new List<MetadataProperty>();
            _references = new Dictionary<MetadataProperty, List<Guid>>();

            _typeParser = new DataTypeSetParser();

            _parser = new ConfigFileParser();
            _parser.Parse(in source, in _converter);

            // result
            target = _target;
            references = _references;

            // dispose private variables
            _target = null;
            _parser = null;
            _property = null;
            _converter = null;
            _references = null;
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
            // Описание типа данных свойства (корневой узел объекта DataTypeSet)

            if (source.Token == TokenType.StartObject)
            {
                _typeParser.Parse(in source, out DataTypeSet type, out List<Guid> references);

                _property.PropertyType = type;

                if (references != null && references.Count > 0)
                {
                    _references.Add(_property, references); //TODO: store in PropertyType !?
                }
            }
        }
        private void PropertyUsage(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.PropertyUsage = (PropertyUsage)source.GetInt32();
        }
    }
}