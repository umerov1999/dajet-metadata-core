using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class SharedPropertyParser
    {
        private readonly ConfigFileParser _parser = new ConfigFileParser();
        private ConfigFileReader Reader { get; }
        public SharedPropertyParser(ConfigFileReader reader)
        {
            Reader = reader;
            ConfigureConfigFileConverter();
        }
        public void Parse(in InfoBase context, in SharedProperty target)
        {
            _infoBase = context;
            _property = target;
            ConfigureDbName();
            _parser.Parse(Reader, _converter);
        }

        int _count = 0;
        InfoBase _infoBase;
        SharedProperty _property;
        ConfigFileConverter _converter;
        private void ConfigureConfigFileConverter()
        {
            _converter = new ConfigFileConverter();

            _converter[1][1][1][1][2] += Name;
            _converter[1][1][1][1][3][2] += Alias;
            _converter[1][6] += AutomaticUsage;
            _converter[1][2][1] += UsageSettings; // количество объектов метаданных, у которых значение использования общего реквизита не равно "Автоматически"
            _converter[1][1][1][2] += PropertyType; // описание допустимых типов данных (объект)
        }
        private void ConfigureDbName()
        {
            //if (_infoBase.DbNames.Lookup.TryGetValue(_property.FileName, out DbName info))
            //{
            //    _property.DbName = DbNamesFactory.CreateDbName(Reader.DatabaseProvider, info.DbName, info.Code);
            //}
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.Name = source.Value;
        }
        private void Alias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.Alias = source.Value;
        }
        private void AutomaticUsage(in ConfigFileReader source, in CancelEventArgs args)
        {
            _property.AutomaticUsage = (AutomaticUsage)source.GetInt32();
        }
        private void UsageSettings(in ConfigFileReader source, in CancelEventArgs args)
        {
            _count = source.GetInt32(); // количество настроек использования общего реквизита

            Guid uuid; // file name объекта метаданных, для которого используется настройка
            int usage; // значение настройки использования общего реквизита объектом метаданных

            while (_count > 0)
            {
                _ = source.Read(); // [2] (1.2.2) 0221aa25-8e8c-433b-8f5b-2d7fead34f7a
                uuid = source.GetUuid(); // file name объекта метаданных
                if (uuid == Guid.Empty) { throw new FormatException(); }

                _ = source.Read(); // [2] (1.2.3) { Начало объекта настройки
                _ = source.Read(); // [3] (1.2.3.0) 2
                _ = source.Read(); // [3] (1.2.3.1) 1
                usage = source.GetInt32(); // настройка использования общего реквизита
                if (usage == -1) { throw new FormatException(); }
                _ = source.Read(); // [3] (1.2.3.2) 00000000-0000-0000-0000-000000000000
                _ = source.Read(); // [2] (1.2.3) } Конец объекта настройки

                _property.UsageSettings.Add(uuid, (SharedPropertyUsage)usage);

                _count--; // Конец чтения настройки для объекта метаданных
            }
        }
        private void PropertyType(in ConfigFileReader source, in CancelEventArgs args)
        {
            if (source.Token == TokenType.EndObject)
            {
                return;
            }

            new DataTypeSetParser().Parse(in source, out DataTypeSet target);
            bool test = target.IsMultipleType;
            // TODO: add property DataTypeSet to MetadataProperty class ???
            // TODO: Configurator.ConfigureDatabaseFields(property); !!!
        }
    }
}