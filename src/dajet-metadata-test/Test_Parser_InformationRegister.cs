using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_InformationRegister
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=dajet-metadata-pg;Username=postgres;Password=postgres;";
        [TestMethod] public void MS_TEST()
        {
            MetadataService service = new();
            
            service.OpenInfoBase(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, out InfoBase infoBase);

            string metadataName = "РегистрСведений.ТестовыйРегистрСведений";

            MetadataObject @object = service.GetMetadataObject(in infoBase, metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
            }
            else
            {
                ShowInformationRegister((InformationRegister)@object);
            }
        }
        [TestMethod] public void PG_TEST()
        {
            //MetadataObject target;
            //Guid uuid = new Guid("f6d7a041-3a57-457c-b303-ff888c9e98b7"); // Идентификатор объекта метаданных

            //using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTables.Config, uuid))
            //{
            //    Parser.Parse(in reader, out target, out Dictionary<MetadataProperty, List<Guid>> references);
            //}

            //if (target == null)
            //{
            //    Console.WriteLine($"{uuid} is not found");
            //}
            //else
            //{
            //    ShowInformationRegister((InformationRegister)target);
            //}
        }
        
        private void ShowInformationRegister(InformationRegister register)
        {
            Console.WriteLine($"Uuid: {register.Uuid}");
            Console.WriteLine($"Name: {register.Name}");
            Console.WriteLine($"Alias: {register.Alias}");
            Console.WriteLine($"Periodicity: {register.Periodicity}");
            Console.WriteLine($"UseRecorder: {register.UseRecorder}");
            
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

            if (type.IsMultipleType)
            {
                Console.WriteLine($"  * MULTIPLE");
                if (type.CanBeString) Console.WriteLine($"    - String ({type.StringLength}) {type.StringKind}");
                if (type.CanBeBoolean) Console.WriteLine("    - Boolean");
                if (type.CanBeNumeric) Console.WriteLine($"    - Numeric ({type.NumericPrecision},{type.NumericScale}) {type.NumericKind}");
                if (type.CanBeDateTime) Console.WriteLine($"    - DateTime ({type.DateTimePart})");
                if (type.CanBeReference) Console.WriteLine($"    - Reference ({(type.Reference == Guid.Empty ? "multiple" : "single")})");
                return;
            }

            if (!type.IsMultipleType)
            {
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
                    Console.WriteLine($"  * {type}");
                }
            }
        }
    }
}