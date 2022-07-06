using DaJet.Metadata;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaJet.Data.Mapping.Test
{
    [TestClass] public class UnitTest1
    {
        private readonly InfoBase _infoBase;
        private readonly MetadataService service = new();
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet_flow_main;Integrated Security=True;Encrypt=False;";
        public UnitTest1()
        {
            MetadataServiceOptions options = new()
            {
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SQLServer
            };

            service.Configure(options);

            if (!service.TryOpenInfoBase(out _infoBase, out string error))
            {
                throw new InvalidOperationException($"Failed to open info base: {error}");
            }
        }
        [TestMethod] public void Test_Method_1()
        {
            string metadataName = "Документ.ЗаказКлиента";

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            EntityDataMapper mapper = new(service, _infoBase.YearOffset, @object as ApplicationObject);

            var entity = mapper.Select(new Guid("8D40CF9C-935C-8ECC-11EC-F65DA01F51E4"));

            foreach (var property in entity)
            {
                Console.WriteLine($"{property.Key} = {property.Value}");
            }
        }
    }
}