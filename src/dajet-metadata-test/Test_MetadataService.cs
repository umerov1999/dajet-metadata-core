using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_MetadataService
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True;Encrypt=False;";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";

        [TestMethod] public void MS_TryOpenInfoBase()
        {
            MetadataServiceOptions options = new()
            {
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SQLServer
            };

            MetadataService service = new(options);

            if (!service.TryOpenInfoBase(out InfoBase infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }

            string metadataName = "РегистрСведений.ВходящаяОчередьRabbitMQ";

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
            }
            else
            {
                Console.WriteLine($"Metadata object \"{@object.Name}\" is found successfully.");
            }
        }
        [TestMethod] public void PG_TryOpenInfoBase()
        {
            MetadataServiceOptions options = new()
            {
                ConnectionString = PG_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.PostgreSQL
            };

            MetadataService service = new(options);

            if (!service.TryOpenInfoBase(out InfoBase infoBase, out string error))
            {
                Console.WriteLine($"Failed to open info base: {error}");
                return;
            }

            string metadataName = "РегистрСведений.ВходящаяОчередьRabbitMQ";

            Stopwatch watch = new();
            watch.Start();
            MetadataObject @object = service.GetMetadataObject(metadataName);
            watch.Stop();
            Console.WriteLine($"1 = {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            @object = service.GetMetadataObject(metadataName);
            watch.Stop();
            Console.WriteLine($"2 = {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            @object = service.GetMetadataObject(MetadataTypes.InformationRegister, @object.Uuid);
            watch.Stop();
            Console.WriteLine($"3 = {watch.ElapsedMilliseconds} ms");

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
            }
            else
            {
                Console.WriteLine($"Metadata object \"{@object.Name}\" is found successfully.");
            }
        }
    }
}