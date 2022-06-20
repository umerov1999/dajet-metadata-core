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
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        private InfoBaseParser InfoBaseParser { get; } = new InfoBaseParser();
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
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                root = new RootFileParser().Parse(in reader);
            }

            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, root))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\config.txt");
        }
        [TestMethod] public void MS_WriteDBSchemaToFile()
        {
            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.DBSchema))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\db-schema-ms.txt");
        }
        [TestMethod] public void PG_WriteDBSchemaToFile()
        {
            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTables.DBSchema))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\db-schema-pg.txt");
        }

        [TestMethod] public void MS_InfoBase_Only()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase);
            }

            ShowInfoBase(in infoBase);
        }
        [TestMethod] public void PG_InfoBase_Only()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            InfoBase infoBase;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase);
            }

            ShowInfoBase(in infoBase);
        }

        [TestMethod] public void MS_InfoBase_And_Metadata()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, out collections);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in collections);
        }
        [TestMethod] public void PG_InfoBase_And_Metadata()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, out collections);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in collections);
        }

        [TestMethod] public void MS_InfoBase_And_Names()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, ConfigFiles.Root))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, out collections);
            }

            MetadataEntry reference;
            ReferenceParser parser = new();

            foreach (var item in collections)
            {
                Console.WriteLine($"{item.Key}");

                foreach (Guid uuid in item.Value)
                {
                    using (ConfigFileReader reader = new ConfigFileReader(
                        DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTables.Config, uuid))
                    {
                        reference = parser.Parse(in reader, item.Key, out string name);

                        Console.WriteLine($"{{{reference.ReferenceUuid}}} {name}");
                    }
                }
            }
        }
    }
}