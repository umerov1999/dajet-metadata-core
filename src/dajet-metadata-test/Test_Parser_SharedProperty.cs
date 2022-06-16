using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_SharedProperty
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True;Encrypt=False;";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=dajet-metadata-pg;Username=postgres;Password=postgres;";

        private readonly MetadataService service = new();

        private InfoBase _infoBase;

        [TestMethod] public void MS_TEST()
        {
            service.OpenInfoBase(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, out _infoBase);
            TEST();
        }
        [TestMethod] public void PG_TEST()
        {
            service.OpenInfoBase(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, out _infoBase);
            TEST();
        }
        private void TEST()
        {
            string metadataName = "ОбщийРеквизит.ОбщийРеквизит1";

            MetadataObject @object = service.GetMetadataObject(in _infoBase, metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
            }
            else
            {
                ShowMetadataObject((SharedProperty)@object);
            }
        }
        private void ShowMetadataObject(SharedProperty @object)
        {
            Console.WriteLine($"Uuid: {@object.Uuid}");
            Console.WriteLine($"Name: {@object.Name}");
            Console.WriteLine($"Alias: {@object.Alias}");
            Console.WriteLine($"Comment: {@object.Comment}");
            
            Console.WriteLine("PropertyType:");

            ShowDataTypeSet(@object.PropertyType);
        }
        private void ShowDataTypeSet(DataTypeSet type)
        {
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
    }
}