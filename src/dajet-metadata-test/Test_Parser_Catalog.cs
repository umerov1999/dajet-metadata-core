using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_Catalog
    {
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=trade_11_2_3_159_demo;Integrated Security=True;Encrypt=False;";
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=accounting_3_0_72_72_demo;Integrated Security=True;Encrypt=False;";
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True;Encrypt=False;";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=dajet-metadata-pg;Username=postgres;Password=postgres;";

        private readonly MetadataService service = new();

        private InfoBase _infoBase;

        [TestMethod] public void MS_TEST()
        {
            InfoBaseOptions options = new()
            {
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SqlServer
            };

            service.Configure(options);

            if (!service.TryOpenInfoBase(out _infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }

            TEST();
        }
        [TestMethod] public void PG_TEST()
        {
            InfoBaseOptions options = new()
            {
                ConnectionString = PG_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.PostgreSql
            };

            service.Configure(options);

            if (!service.TryOpenInfoBase(out _infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }

            TEST();
        }
        private void TEST()
        {
            string metadataName = "Справочник.Номенклатура"; //"СправочникПредопределённые"; // ТестовыйСправочник

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
            }
            else
            {
                ShowMetadataObject((Catalog)@object);
            }

            Console.WriteLine();
            Console.WriteLine("Change table: " + service.GetChangeTableName(@object));

            Console.WriteLine();
            ShowDatabaseNames((Catalog)@object);
        }
        private void ShowDatabaseNames(Catalog catalog)
        {
            Console.WriteLine($"TableName: {catalog.TableName}");

            Console.WriteLine("Properties:");

            foreach (MetadataProperty property in catalog.Properties)
            {
                Console.WriteLine($"- {property.Name} ({property.DbName})");

                foreach (DatabaseField field in property.Fields)
                {
                    Console.WriteLine($"--- {field.Name} ({field.TypeName})");
                }
            }
        }
        private void ShowMetadataObject(Catalog @object)
        {
            Console.WriteLine($"Uuid: {@object.Uuid}");
            Console.WriteLine($"Name: {@object.Name}");
            Console.WriteLine($"Alias: {@object.Alias}");
            
            Console.WriteLine("Properties:");

            foreach (MetadataProperty property in @object.Properties)
            {
                ShowProperty(property);
            }

            Console.WriteLine("TableParts:");

            foreach (TablePart table in @object.TableParts)
            {
                ShowTablePart(table);
            }
        }
        private void ShowTablePart(TablePart table)
        {
            Console.WriteLine($"Uuid: {table.Uuid}");
            Console.WriteLine($"Name: {table.Name}");

            Console.WriteLine("Properties:");

            foreach (MetadataProperty property in table.Properties)
            {
                ShowProperty(property);
            }
        }
        private void ShowProperty(MetadataProperty property)
        {
            Console.WriteLine($"- {property.Purpose}: {property.Name} ({property.Alias}) {{{property.Uuid}}} {property.PropertyUsage}");

            DataTypeSet type = property.PropertyType;
            if (type == null)
            {
                return;
            }

            string name = string.Empty;
            
            //if (type.CanBeReference && type.Reference != Guid.Empty)
            //{
            //    MetadataObject @object = service.GetMetadataObjectByReference(in _infoBase, type.Reference);
            //    if (@object != null)
            //    {
            //        name = MetadataTypes.ResolveNameRu(@object.Uuid) + "." + @object.Name;
            //    }
            //}

            if (type.IsMultipleType)
            {
                Console.WriteLine($"  * MULTIPLE");
                if (type.CanBeString) Console.WriteLine($"    - String ({type.StringLength}) {type.StringKind}");
                if (type.CanBeBoolean) Console.WriteLine("    - Boolean");
                if (type.CanBeNumeric) Console.WriteLine($"    - Numeric ({type.NumericPrecision},{type.NumericScale}) {type.NumericKind}");
                if (type.CanBeDateTime) Console.WriteLine($"    - DateTime ({type.DateTimePart})");
                if (type.CanBeReference) Console.WriteLine($"    - Reference ({(type.Reference == Guid.Empty ? "multiple" : "single")}) {name}");
                return;
            }

            if (type.IsUuid)
            {
                Console.WriteLine("  * UUID");
            }
            else if (type.IsBinary)
            {
                Console.WriteLine("  * Binary");
            }
            else if (type.IsValueStorage)
            {
                Console.WriteLine("  * ValueStorage");
            }
            else if (type.CanBeBoolean)
            {
                Console.WriteLine("  * Boolean");
            }
            else if (type.CanBeString)
            {
                Console.WriteLine($"  * String ({type.StringLength}) {type.StringKind}");
            }
            else if (type.CanBeNumeric)
            {
                Console.WriteLine($"  * Numeric ({type.NumericPrecision},{type.NumericScale}) {type.NumericKind}");
            }
            else if (type.CanBeDateTime)
            {
                Console.WriteLine($"  * DateTime ({type.DateTimePart})");
            }
            else if (type.CanBeReference)
            {
                Console.WriteLine($"  * {type} [{name}]");
            }
        }

        ISqlMetadataReader SqlMetadataReader = new SqlMetadataReader();
        IMetadataCompareAndMergeService CompareMergeService = new MetadataCompareAndMergeService(); // comparator
        [TestMethod] public void MS_Compare_With_Database()
        {
            InfoBaseOptions options = new()
            {
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SqlServer
            };

            service.Configure(options);

            if (!service.TryOpenInfoBase(out _infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }

            SqlMetadataReader.UseDatabaseProvider(options.DatabaseProvider);
            SqlMetadataReader.UseConnectionString(options.ConnectionString);

            int count = 0;
            List<string> delete;
            List<string> insert;

            using (StreamWriter stream = new StreamWriter(@"C:\temp\Test_Catalogs.txt", false, Encoding.UTF8))
            {
                foreach (MetadataObject metadata in service.GetMetadataObjects(MetadataTypes.Catalog))
                {
                    if (metadata is not ApplicationObject @object)
                    {
                        continue; // should not happen
                    }

                    count++;

                    bool result = CompareWithDatabase(@object, out delete, out insert);
                    
                    if (!result)
                    {
                        LogResult(stream, @object, delete, insert);
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
                            LogResult(stream, tablePart, delete, insert);
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

            List<SqlFieldInfo> sqlFields = SqlMetadataReader.GetSqlFieldsOrderedByName(@object.TableName);

            if (sqlFields.Count == 0)
            {
                return false;
            }

            List<string> targetFields = CompareMergeService.PrepareComparison(@object.Properties);
            List<string> sourceFields = CompareMergeService.PrepareComparison(sqlFields);

            CompareMergeService.Compare(targetFields, sourceFields, out delete, out insert);

            return (delete.Count + insert.Count) == 0;
        }
        private void LogResult(StreamWriter stream, ApplicationObject @object, List<string> delete, List<string> insert)
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