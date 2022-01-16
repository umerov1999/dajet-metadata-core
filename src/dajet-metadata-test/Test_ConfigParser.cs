using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_ConfigParser
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";

        [TestMethod] public void MS_ROOT()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = ConfigFileParser.Parse(in reader).GetUuid(1).ToString();
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                new InfoBaseParser().Parse(in reader, out infoBase, out collections);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in collections);
        }
        [TestMethod] public void PG_ROOT()
        {
            InfoBaseParser parser = new InfoBaseParser();

            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = ConfigFileParser.Parse(in reader).GetUuid(1).ToString();
            }

            InfoBase infoBase;
            Dictionary<Guid, List<Guid>> collections;

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                parser.Parse(in reader, out infoBase, out collections);
            }

            ShowInfoBase(in infoBase);
            ShowCollections(in collections);
        }

        [TestMethod] public void MS_ParseByName()
        {
            string rootFile = null;
            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, "root"))
            {
                rootFile = ConfigFileParser.Parse(in reader).GetUuid(1).ToString();
            }

            ApplicationObject target;
            string name = "ÂõîäÿùàÿÎ÷åðåäüRabbitMQ";

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                new InfoBaseParser().ParseByName(in reader, MetadataRegistry.InformationRegisters, in name , out target);
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
                rootFile = ConfigFileParser.Parse(in reader).GetUuid(1).ToString();
            }

            ApplicationObject target;
            Guid uuid = new Guid("f6d7a041-3a57-457c-b303-ff888c9e98b7");

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, rootFile))
            {
                new InfoBaseParser().ParseByUuid(in reader, MetadataRegistry.InformationRegisters, uuid, out target);
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


        private void ShowInfoBase(in InfoBase infoBase)
        {
            foreach (PropertyInfo property in typeof(InfoBase).GetProperties())
            {
                Console.WriteLine($"{property.Name, -20} = {property.GetValue(infoBase)}");
            }
        }
        private void ShowCollections(in Dictionary<Guid, List<Guid>> collections)
        {
            foreach (var item in collections)
            {
                Console.WriteLine($"{item.Key}");

                foreach (Guid uuid in item.Value)
                {
                    Console.WriteLine($"{uuid, 40}");
                }
            }
        }
    }
}