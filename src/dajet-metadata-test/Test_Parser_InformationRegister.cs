using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_Parser_InformationRegister
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";
        private InformationRegisterParser Parser { get; }
        public Test_Parser_InformationRegister()
        {
            if (!MetadataParserFactory.TryGetParser(MetadataRegistry.InformationRegisters, out IMetadataObjectParser parser))
            {
                throw new Exception("InformationRegister parser is not found");
            }

            Parser = parser as InformationRegisterParser;

            if (Parser == null)
            {
                throw new Exception("Failed to get InformationRegister parser");
            }
        }
        [TestMethod] public void MS_ParseByUuid()
        {
            MetadataObject target;
            Guid uuid = new Guid("f6d7a041-3a57-457c-b303-ff888c9e98b7"); // Идентификатор объекта метаданных

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, ConfigTableNames.Config, uuid))
            {
                Parser.Parse(in reader, out target);
            }

            if (target == null)
            {
                Console.WriteLine($"{uuid} is not found");
            }
            else
            {
                ShowInformationRegister((InformationRegister)target);
            }
        }
        [TestMethod] public void PG_ParseByUuid()
        {
            MetadataObject target;
            Guid uuid = new Guid("f6d7a041-3a57-457c-b303-ff888c9e98b7"); // Идентификатор объекта метаданных

            using (ConfigFileReader reader = new ConfigFileReader(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, ConfigTableNames.Config, uuid))
            {
                Parser.Parse(in reader, out target);
            }

            if (target == null)
            {
                Console.WriteLine($"{uuid} is not found");
            }
            else
            {
                ShowInformationRegister((InformationRegister)target);
            }
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
                    if (type.References != null && type.References.Count > 0)
                    {
                        Console.WriteLine("  * Reference");
                        foreach (Guid uuid in type.References)
                        {
                            Console.WriteLine($"    # {uuid}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  * Reference (null)");
                    }
                }
                else
                {
                    Console.WriteLine($"  * {type}");
                }
            }
        }
    }
}