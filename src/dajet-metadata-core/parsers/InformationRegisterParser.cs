using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.ComponentModel;

namespace DaJet.Metadata.Parsers
{
    public sealed class InformationRegisterParser : IMetadataObjectParser
    {
        private string _name;
        private InformationRegister _target;
        private ConfigFileConverter _converter;

        private void ConfigureConfigFileConverter()
        {
            _converter = new ConfigFileConverter();

            _converter[1][15][1][2] += Name;
            _converter[1][15][1][3][2] += Alias;
            _converter[1][18] += Periodicity;
            _converter[1][19] += UseRecorder;
        }
        public void Parse(in ConfigFileReader reader, out MetadataObject target)
        {
            Parse(in reader, null, out target);
        }
        public void Parse(in ConfigFileReader reader, in string name, out MetadataObject target)
        {
            ConfigureConfigFileConverter();

            _target = new InformationRegister()
            {
                Uuid = new Guid(reader.FileName)
            };

            _name = name; // filter

            ConfigFileParser.Parse(in reader, in _converter);

            target = _target; // result

            // TODO: Dispose()
            _name = null;
            _target = null;
            _converter = null;
        }
        private void Name(in ConfigFileReader source, in CancelEventArgs args)
        {
            // apply filter by name
            if (_name != null && _name != source.Value)
            {
                _target = null;
                args.Cancel = true;
                return;
            }

            _target.Name = source.Value;
        }
        private void Alias(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Alias = source.Value;
        }
        private void Periodicity(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.Periodicity = (RegisterPeriodicity)source.GetInt32();
        }
        private void UseRecorder(in ConfigFileReader source, in CancelEventArgs args)
        {
            _target.UseRecorder = (source.GetInt32() != 0);
        }
    }
}

//if (register.Periodicity != RegisterPeriodicity.None)
//{
//    Configurator.ConfigurePropertyПериод(register);
//}
//if (register.UseRecorder)
//{
//    // Свойство "Регистратор" конфигурируется при загрузке документов
//    Configurator.ConfigurePropertyНомерЗаписи(register);
//    Configurator.ConfigurePropertyАктивность(register);
//}

//// 4 - коллекция измерений регистра сведений
//ConfigObject properties = configObject.GetObject(new int[] { 4 });
//// 4.0 = 13134203-f60b-11d5-a3c7-0050bae0a776 - идентификатор коллекции измерений
//Guid propertiesUuid = configObject.GetUuid(new int[] { 4, 0 });
//if (propertiesUuid == new Guid("13134203-f60b-11d5-a3c7-0050bae0a776"))
//{
//    Configurator.ConfigureProperties(register, properties, PropertyPurpose.Dimension);
//}

//// 3 - коллекция ресурсов регистра сведений
//properties = configObject.GetObject(new int[] { 3 });
//// 3.0 = 13134202-f60b-11d5-a3c7-0050bae0a776 - идентификатор коллекции ресурсов
//propertiesUuid = configObject.GetUuid(new int[] { 3, 0 });
//if (propertiesUuid == new Guid("13134202-f60b-11d5-a3c7-0050bae0a776"))
//{
//    Configurator.ConfigureProperties(register, properties, PropertyPurpose.Measure);
//}

//// 7 - коллекция реквизитов регистра сведений
//properties = configObject.GetObject(new int[] { 7 });
//// 7.0 = a2207540-1400-11d6-a3c7-0050bae0a776 - идентификатор коллекции реквизитов
//propertiesUuid = configObject.GetUuid(new int[] { 7, 0 });
//if (propertiesUuid == new Guid("a2207540-1400-11d6-a3c7-0050bae0a776"))
//{
//    Configurator.ConfigureProperties(register, properties, PropertyPurpose.Property);
//}

//Configurator.ConfigureSharedProperties(register);