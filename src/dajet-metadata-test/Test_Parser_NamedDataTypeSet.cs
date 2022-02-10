using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_NamedDataTypeSet
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        private InfoBaseParser GetInfoBaseParser()
        {
            return new InfoBaseParser();
        }
        private NamedDataTypeSetParser GetNamedDataTypeSetParser()
        {
            if (!MetadataParserFactory.TryGetParser(MetadataTypes.NamedDataTypeSet, out IMetadataObjectParser parser))
            {
                throw new Exception("NamedDataTypeSet parser is not found");
            }
            return parser as NamedDataTypeSetParser;
        }
        private Guid GetRootFileUuid(DatabaseProvider provider, string connectionString)
        {
            Guid root;
            using (ConfigFileReader reader = new ConfigFileReader(provider, connectionString, ConfigTables.Config, "root"))
            {
                root = new RootFileParser().Parse(in reader);
            }
            return root;
        }
        private void GetMetadata(DatabaseProvider provider, string connectionString, out InfoBase infoBase, out Dictionary<Guid, List<Guid>> metadata)
        {
            Guid root = GetRootFileUuid(provider, connectionString);

            InfoBaseParser parser = GetInfoBaseParser();

            using (ConfigFileReader reader = new ConfigFileReader(provider, connectionString, ConfigTables.Config, root))
            {
                parser.Parse(in reader, out infoBase, out metadata);
            }
        }
        //private void GetMetadataByUuid(DatabaseProvider provider, string connectionString, Guid guid, out MetadataObject target)
        //{
        //    Guid root = GetRootFileUuid(provider, connectionString);

        //    InfoBaseParser parser = GetInfoBaseParser();

        //    using (ConfigFileReader reader = new ConfigFileReader(provider, connectionString, ConfigTables.Config, root))
        //    {
        //        parser.ParseByUuid(in reader, MetadataTypes.NamedDataTypeSet, guid, out target);
        //    }
        //}
        //private void GetMetadataByName(DatabaseProvider provider, string connectionString, in string name, out MetadataObject target)
        //{
        //    Guid root = GetRootFileUuid(provider, connectionString);

        //    InfoBaseParser parser = GetInfoBaseParser();

        //    using (ConfigFileReader reader = new ConfigFileReader(provider, connectionString, ConfigTables.Config, root))
        //    {
        //        parser.ParseByName(in reader, MetadataTypes.NamedDataTypeSet, in name, out target);
        //    }
        //}

        #region "SHOW TEST RESULTS"

        private void ShowInfoBase(in InfoBase infoBase)
        {
            Console.WriteLine($"{infoBase.Name} ({infoBase.Alias}) {infoBase.AppConfigVersion}");
            Console.WriteLine();
        }
        private void ShowMetadataObject(in NamedDataTypeSet metaObject)
        {
            Console.WriteLine($"{metaObject.Name} ({metaObject.Alias}) {{{metaObject.Uuid}}} {metaObject.Comment}");
            ShowDataTypeSet(metaObject.DataTypeSet);
            Console.WriteLine();
        }
        private void ShowDataTypeSet(DataTypeSet type)
        {
            if (type.IsMultipleType)
            {
                ShowMultipleDataTypeSet(type);
            }
            else
            {
                ShowSingleDataTypeSet(type);
            }
        }
        private void ShowSingleDataTypeSet(DataTypeSet type)
        {
            if (type.IsUuid)
            {
                Console.WriteLine("- UUID");
            }
            else if (type.IsBinary)
            {
                Console.WriteLine("- Binary");
            }
            else if (type.IsValueStorage)
            {
                Console.WriteLine("- ValueStorage");
            }
            else if (type.CanBeBoolean)
            {
                Console.WriteLine("- Boolean");
            }
            else if (type.CanBeString)
            {
                Console.WriteLine($"- String ({type.StringLength}) {type.StringKind}");
            }
            else if (type.CanBeNumeric)
            {
                Console.WriteLine($"- Numeric ({type.NumericPrecision},{type.NumericScale}) {type.NumericKind}");
            }
            else if (type.CanBeDateTime)
            {
                Console.WriteLine($"- DateTime ({type.DateTimePart})");
            }
            else if (type.CanBeReference)
            {
                if (type.References != null && type.References.Count > 0)
                {
                    Console.WriteLine("- Reference");
                    foreach (Guid uuid in type.References)
                    {
                        Console.WriteLine($"   # {uuid}");
                    }
                }
                else
                {
                    Console.WriteLine($"- Reference (null)");
                }
            }
            else
            {
                Console.WriteLine($"- {type}");
            }
        }
        private void ShowMultipleDataTypeSet(DataTypeSet type)
        {
            if (type.CanBeBoolean)
            {
                Console.WriteLine("- Boolean");
            }
            else if (type.CanBeString)
            {
                Console.WriteLine($"- String ({type.StringLength}) {type.StringKind}");
            }
            else if (type.CanBeNumeric)
            {
                Console.WriteLine($"- Numeric ({type.NumericPrecision},{type.NumericScale}) {type.NumericKind}");
            }
            else if (type.CanBeDateTime)
            {
                Console.WriteLine($"- DateTime ({type.DateTimePart})");
            }
            else if (type.CanBeReference)
            {
                if (type.References != null && type.References.Count > 0)
                {
                    Console.WriteLine("- Reference");

                    foreach (Guid uuid in type.References)
                    {
                        Console.WriteLine($"   # {uuid}");
                    }
                }
                else
                {
                    Console.WriteLine($"- Reference (null)");
                }
            }
            else
            {
                Console.WriteLine($"- {type}");
            }
        }

        #endregion

        private void ParseAll(DatabaseProvider provider, string connectionString)
        {
            NamedDataTypeSetParser parser = GetNamedDataTypeSetParser();

            GetMetadata(provider, connectionString, out InfoBase infoBase, out Dictionary<Guid, List<Guid>> metadata);

            if (!metadata.TryGetValue(MetadataTypes.NamedDataTypeSet, out List<Guid> items))
            {
                Console.WriteLine($"NamedDataTypeSet list is empty.");
                return;
            }

            ShowInfoBase(in infoBase);

            foreach (Guid guid in items)
            {
                using (ConfigFileReader reader = new ConfigFileReader(provider, connectionString, ConfigTables.Config, guid))
                {
                    parser.Parse(in reader, out MetadataObject item);

                    NamedDataTypeSet metaObject = item as NamedDataTypeSet;

                    ShowMetadataObject(in metaObject);
                }
            }
        }
        //private void ParseByUuid(DatabaseProvider provider, string connectionString, Guid guid)
        //{
        //    GetMetadataByUuid(provider, connectionString, guid, out MetadataObject item);

        //    NamedDataTypeSet metaObject = item as NamedDataTypeSet;

        //    ShowMetadataObject(in metaObject);
        //}
        //private void ParseByName(DatabaseProvider provider, string connectionString, string name)
        //{
        //    GetMetadataByName(provider, connectionString, in name, out MetadataObject item);

        //    NamedDataTypeSet metaObject = item as NamedDataTypeSet;

        //    ShowMetadataObject(in metaObject);
        //}
        
        [TestMethod] public void MS_ParseAll()
        {
            ParseAll(DatabaseProvider.SQLServer, MS_CONNECTION_STRING);
        }
        [TestMethod] public void MS_ParseByUuid()
        {
            //ParseByUuid(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, new Guid("deb69010-80b0-4e28-acc7-8c50e8e6f873"));
        }
        [TestMethod] public void MS_ParseByName()
        {
            //ParseByName(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, "ТипыОбъектовМДЛП");
        }
        
        [TestMethod] public void PG_ParseAll()
        {
            ParseAll(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING);
        }
        [TestMethod] public void PG_ParseByUuid()
        {
            //ParseByUuid(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, new Guid("deb69010-80b0-4e28-acc7-8c50e8e6f873"));
        }
        [TestMethod] public void PG_ParseByName()
        {
            //ParseByName(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, "ТипыОбъектовМДЛП");
        }
    }
}