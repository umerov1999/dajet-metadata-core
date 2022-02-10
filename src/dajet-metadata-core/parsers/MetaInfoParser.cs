using DaJet.Metadata.Core;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class MetaInfoParser
    {
        private readonly ConfigFileParser _parser = new ConfigFileParser();

        private Guid _uuid = Guid.Empty;
        private string _name = string.Empty;
        private ConfigFileConverter _converter;
        public MetaInfo Parse(in ConfigFileReader reader, Guid type)
        {
            ConfigureConfigFileConverter(type);

            _parser.Parse(in reader, in _converter);

            MetaInfo result = new MetaInfo(_uuid, _name);

            _converter = null;
            _uuid = Guid.Empty;
            _name = string.Empty;

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
                _converter[1][13][1][2] += Name; // Имя объекта конфигурации
            }
        }
        private void Uuid(in ConfigFileReader source, in CancelEventArgs args)
        {
            _uuid = source.GetUuid();
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            _name = source.Value;

            args.Cancel = true;
        }
    }
}