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
    [TestClass] public class Test_ConfigParser
    {
        private readonly ConfigFileParser _parser = new ConfigFileParser();
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        private InfoBaseParser InfoBaseParser { get; }
        public Test_ConfigParser()
        {
            if (!MetadataParserFactory.TryGetParser(MetadataRegistry.Root, out IMetadataObjectParser parser))
            {
                throw new Exception("InfoBase parser is not found");
            }

            InfoBaseParser = parser as InfoBaseParser;

            if (InfoBaseParser == null)
            {
                throw new Exception("Failed to get InfoBase parser");
            }
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
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                root = new RootFileParser().Parse(in reader);
            }

            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(
                DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, root))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\config.txt");
        }
        [TestMethod] public void MS_WriteDBSchemaToFile()
        {
            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.DBSchema))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\db-schema-ms.txt");
        }
        [TestMethod] public void PG_WriteDBSchemaToFile()
        {
            ConfigObject config;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTableNames.DBSchema))
            {
                config = new ConfigFileParser().Parse(in reader);
            }

            new ConfigFileWriter().Write(config, "C:\\temp\\db-schema-pg.txt");
        }

        [TestMethod] public void MS_ROOT()
        {
            Guid rootFile;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = new RootFileParser().Parse(in reader);
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, out collections);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in collections);
        }
        [TestMethod] public void PG_ROOT()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                InfoBaseParser.Parse(in reader, out infoBase, out collections);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in collections);
        }

        [TestMethod] public void MS_ParseByName()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            MetadataObject target;
            string name = "ÂõîäÿùàÿÎ÷åðåäüRabbitMQ";

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                InfoBaseParser.ParseByName(in reader, MetadataRegistry.InformationRegisters, in name , out target);
            }

            if (target == null)
            {
                Console.WriteLine($"{name} is not found");
            }
            else
            {
                Console.WriteLine($"{name} {{{target.Uuid}}} is found successfully");
            }
        }
        [TestMethod] public void MS_ParseByUuid()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = _parser.Parse(in reader).GetString(1);
            }

            MetadataObject target;
            Guid uuid = new Guid("f6d7a041-3a57-457c-b303-ff888c9e98b7");

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                InfoBaseParser.ParseByUuid(in reader, MetadataRegistry.InformationRegisters, uuid, out target);
            }

            if (target == null)
            {
                Console.WriteLine($"{uuid} is not found");
            }
            else
            {
                Console.WriteLine($"{target.Name} {{{target.Uuid}}} is found successfully");
            }
        }
    }
}