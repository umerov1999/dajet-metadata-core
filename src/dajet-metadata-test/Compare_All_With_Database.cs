using DaJet.Metadata;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaJet.Database.Test
{
    [TestClass] public class Compare_All_With_Database
    {
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=trade_11_2_3_159_demo;Integrated Security=True;Encrypt=False;";
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=accounting_3_0_72_72_demo;Integrated Security=True;Encrypt=False;";
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True;Encrypt=False;";
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True;Encrypt=False;";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=dajet-metadata-pg;Username=postgres;Password=postgres;";
        //private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        private InfoBase _infoBase;
        private readonly MetadataService _service = new();
        ISqlMetadataReader _database = new SqlMetadataReader();
        IMetadataCompareAndMergeService _comparator = new MetadataCompareAndMergeService(); // comparator
        [TestMethod] public void MS_Test()
        {
            InfoBaseOptions options = new()
            {
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SqlServer
            };

            _service.Configure(options);
            
            _database.UseDatabaseProvider(options.DatabaseProvider);
            _database.UseConnectionString(options.ConnectionString);

            if (!_service.TryOpenInfoBase(out _infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }
            
            Run_Test();
        }
        [TestMethod] public void PG_Test()
        {
            InfoBaseOptions options = new()
            {
                ConnectionString = PG_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.PostgreSql
            };

            _service.Configure(options);

            _database.UseDatabaseProvider(options.DatabaseProvider);
            _database.UseConnectionString(options.ConnectionString);

            if (!_service.TryOpenInfoBase(out _infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }

            Run_Test();
        }
        private void Run_Test()
        {
            Console.WriteLine($"{_infoBase.Name} - {_infoBase.Alias} [{_infoBase.Comment}] {_infoBase.AppConfigVersion}");

            Dictionary<Guid, string> types = new()
            {
                { MetadataTypes.Catalog, "C:\\temp\\db-tests\\catalogs.txt" },
                { MetadataTypes.Document, "C:\\temp\\db-tests\\documents.txt" },
                { MetadataTypes.Enumeration, "C:\\temp\\db-tests\\enumerations.txt" },
                { MetadataTypes.Publication, "C:\\temp\\db-tests\\publications.txt" },
                { MetadataTypes.Characteristic, "C:\\temp\\db-tests\\characteristics.txt" },
                { MetadataTypes.InformationRegister, "C:\\temp\\db-tests\\information-registers.txt" },
                { MetadataTypes.AccumulationRegister, "C:\\temp\\db-tests\\accumulation-registers.txt" }
            };

            foreach (var type in types)
            {
                Run_Test(type.Key, type.Value);
            }
        }
        private void Run_Test(Guid metadataType, string outputFile)
        {
            int count = 0;
            List<string> delete;
            List<string> insert;

            using (StreamWriter stream = new(outputFile, false, Encoding.UTF8))
            {
                foreach (MetadataObject metadata in _service.GetMetadataObjects(metadataType))
                {
                    if (metadata is not ApplicationObject @object)
                    {
                        continue; // should not happen
                    }

                    count++;

                    bool result = CompareWithDatabase(@object, out delete, out insert);

                    if (!result)
                    {
                        WriteToLogFile(stream, @object, delete, insert);
                    }

                    if (@object is not ITablePartOwner aggregate)
                    {
                        continue; // should not happen
                    }

                    foreach (TablePart tablePart in aggregate.TableParts)
                    {
                        result = CompareWithDatabase(tablePart, out delete, out insert);

                        if (!result)
                        {
                            WriteToLogFile(stream, tablePart, delete, insert);
                        }
                    }
                }
                stream.WriteLine("*******************************");
                stream.WriteLine(count.ToString() + " objects processed.");
            }
        }
        private bool CompareWithDatabase(ApplicationObject @object, out List<string> delete, out List<string> insert)
        {
            delete = new List<string>();
            insert = new List<string>();

            List<SqlFieldInfo> sqlFields = _database.GetSqlFieldsOrderedByName(@object.TableName);

            if (sqlFields.Count == 0)
            {
                return false;
            }

            List<string> targetFields = _comparator.PrepareComparison(@object.Properties);
            List<string> sourceFields = _comparator.PrepareComparison(sqlFields);

            _comparator.Compare(targetFields, sourceFields, out delete, out insert);

            return (delete.Count + insert.Count) == 0;
        }
        private void WriteToLogFile(StreamWriter stream, ApplicationObject @object, List<string> delete, List<string> insert)
        {
            stream.WriteLine("\"" + @object.Name + "\" (" + @object.TableName + "):");

            if (delete.Count > 0)
            {
                stream.WriteLine("  Delete fields:");
                foreach (string field in delete)
                {
                    stream.WriteLine("   - " + field);
                }
            }

            if (insert.Count > 0)
            {
                stream.WriteLine("  Insert fields:");
                foreach (string field in insert)
                {
                    stream.WriteLine("   - " + field);
                }
            }
        }
    }
}