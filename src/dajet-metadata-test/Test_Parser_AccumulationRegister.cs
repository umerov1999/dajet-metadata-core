using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_AccumulationRegister
    {
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
            string metadataName = "РегистрНакопления.РегистрНакопленияОбороты"; // "РегистрНакопления.РегистрНакопленияОстатки";

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
            }
            else
            {
                ShowAccumulationRegister((AccumulationRegister)@object);
            }
        }
        private void ShowAccumulationRegister(AccumulationRegister register)
        {
            Console.WriteLine($"Uuid: {register.Uuid}");
            Console.WriteLine($"Name: {register.Name}");
            Console.WriteLine($"Alias: {register.Alias}");
            Console.WriteLine($"RegisterKind: {register.RegisterKind}");
            Console.WriteLine($"UseSplitter: {register.UseSplitter}");
            
            Console.WriteLine("Properties:");

            foreach (MetadataProperty property in register.Properties)
            {
                ShowProperty(property);
            }
        }
        private void ShowProperty(MetadataProperty property)
        {
            Console.WriteLine($"- {property.Purpose}: {property.Name} ({property.Alias}) {{{property.Uuid}}} ");

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
    }
}