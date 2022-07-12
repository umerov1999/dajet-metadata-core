using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using DaJet.Metadata.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_InfoBase
    {
        private readonly ConfigFileParser _parser = new ConfigFileParser();
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True;Encrypt=False;";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        private InfoBaseParser InfoBaseParser { get; } = new InfoBaseParser();
        private readonly Dictionary<Guid, List<Guid>> _metadata;
        private readonly Dictionary<Guid, IMetadataObjectParser> _parsers;
        public Test_Parser_InfoBase()
        {
            _metadata = new()
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

            _parsers = new() // supported metadata object parsers
            {
                { MetadataTypes.Catalog, new CatalogParser(null) },
                { MetadataTypes.Document, new DocumentParser(null) },
                { MetadataTypes.Enumeration, new EnumerationParser(null) },
                { MetadataTypes.Publication, new PublicationParser(null) },
                { MetadataTypes.Characteristic, new CharacteristicParser(null) },
                { MetadataTypes.InformationRegister, new InformationRegisterParser(null) },
                { MetadataTypes.AccumulationRegister, new AccumulationRegisterParser(null) },
                { MetadataTypes.SharedProperty, new SharedPropertyParser(null) },
                { MetadataTypes.NamedDataTypeSet, new NamedDataTypeSetParser(null) } // since 1C:Enterprise 8.3.3 version
            };
        }
        private void ShowInfoBase(in InfoBase infoBase)
        {
            foreach (PropertyInfo property in typeof(InfoBase).GetProperties())
            {
                Console.WriteLine($"{property.Name,-20} = {property.GetValue(infoBase)}");
            }
        }
        private void ShowCollections(in Dictionary<Guid, List<Guid>> collections)
        {
            foreach (var item in collections)
            {
                Console.WriteLine($"{item.Key}");

                foreach (Guid uuid in item.Value)
                {
                    Console.WriteLine($"{uuid,40}");
                }
            }
        }

        [TestMethod] public void WriteRootConfigToFile()
        {
            Guid root;
            
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                root = new RootFileParser().Parse(in reader);
            }

            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, root))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\config.txt");
        }
        [TestMethod] public void MS_WriteDBSchemaToFile()
        {
            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.DBSchema))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\db-schema-ms.txt");
        }
        [TestMethod] public void PG_WriteDBSchemaToFile()
        {
            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSql, PG_CONNECTION_STRING, ConfigTables.DBSchema))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\db-schema-pg.txt");
        }

        [TestMethod] public void MS_InfoBase_Only()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase);
            }

            ShowInfoBase(in infoBase);
        }
        [TestMethod] public void PG_InfoBase_Only()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSql, PG_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSql, PG_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase);
            }

            ShowInfoBase(in infoBase);
        }

        [TestMethod] public void MS_InfoBase_And_Metadata()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, in _metadata);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in _metadata);
        }
        [TestMethod] public void PG_InfoBase_And_Metadata()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSql, PG_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSql, PG_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, in _metadata);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in _metadata);
        }

        [TestMethod] public void MS_InfoBase_And_Names()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, in _metadata);
            }
            
            foreach (var item in _metadata)
            {
                Console.WriteLine($"{item.Key}");

                if (!_parsers.TryGetValue(item.Key, out IMetadataObjectParser parser))
                {
                    Console.WriteLine($"{{{item.Key}}} parser is not found");
                    continue;
                }

                foreach (Guid uuid in item.Value)
                {
                    using (ConfigFileReader reader = new ConfigFileReader(
                        DatabaseProvider.SqlServer, MS_CONNECTION_STRING, ConfigTables.Config, uuid))
                    {
                        parser.Parse(in reader, out MetadataInfo info);

                        Console.WriteLine($"{{{info.ReferenceUuid}}} {info.Name}");
                    }
                }
            }
        }
    }
}