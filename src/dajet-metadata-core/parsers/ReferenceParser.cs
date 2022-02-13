using DaJet.Metadata.Core;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class ReferenceParser
    {
        private readonly ConfigFileParser _parser = new();

        
        private string _name = string.Empty;
        private Guid _reference = Guid.Empty;
        private Guid _characteristic = Guid.Empty;
        private ConfigFileConverter _converter;
        public ReferenceInfo Parse(in ConfigFileReader reader, Guid type, out string name)
        {
            ConfigureConfigFileConverter(type);

            Guid metadata = new Guid(reader.FileName);

            _parser.Parse(in reader, in _converter);

            ReferenceInfo result = new(type, metadata, _reference, _characteristic);

            name = _name;

            _converter = null;
            _name = string.Empty;
            _reference = Guid.Empty;
            _characteristic = Guid.Empty;

            return result;
        }
        private void ConfigureConfigFileConverter(Guid type)
        {
            _converter = new ConfigFileConverter();

            if (type == MetadataTypes.Catalog || type == MetadataTypes.Document)
            {
                _converter[1][3] += Uuid; // Идентификатор ссылочного типа данных
                _converter[1][9][1][2] += Name; // Имя объекта конфигурации
            }
            else if (type == MetadataTypes.InformationRegister)
            {
                _converter[1][15][1][2] += Name; // Имя объекта конфигурации
            }
            else if (type == MetadataTypes.AccumulationRegister)
            {
                _converter[1][13][1][2] += Name; // Имя объекта конфигурации
            }
            else if (type == MetadataTypes.Enumeration)
            {
                _converter[1][1] += Uuid; // Идентификатор ссылочного типа данных
                _converter[1][5][1][2] += Name; // Имя объекта конфигурации
            }
            else if (type == MetadataTypes.Publication)
            {
                _converter[1][3] += Uuid; // Идентификатор ссылочного типа данных
                _converter[1][12][2] += Name; // Имя объекта конфигурации
            }
            else if (type == MetadataTypes.Characteristic)
            {
                _converter[1][3] += Uuid; // Идентификатор ссылочного типа данных
                _converter[1][9] += CharacteristicUuid; // Идентификатор характеристики
                _converter[1][13][1][2] += Name; // Имя объекта конфигурации
            }
            else if (type == MetadataTypes.NamedDataTypeSet)
            {
                _converter[1][1] += Uuid; // Идентификатор ссылочного типа данных
                _converter[1][3][2] += Name; // Имя объекта конфигурации
            }
        }
        private void Uuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            _reference = source.GetUuid();
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            _name = source.Value;

            args.Cancel = true;
        }
        private void CharacteristicUuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            _characteristic = source.GetUuid();
        }
    }
}