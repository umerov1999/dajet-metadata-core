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
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True;Encrypt=False;";
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
        [TestMethod] public void Test_Select_Entity()
        {
            string metadataName = "Справочник.СправочникИерархическийГруппы";

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object as ApplicationObject
            };
            //options.Filter.Add(new FilterParameter("Ссылка", new Guid("e403d57f-fe02-11ec-9ccf-408d5c93cc8e")));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var entity in list)
            {
                Console.WriteLine();
                foreach (var property in entity)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                }
            }

            Console.WriteLine();
            Console.WriteLine("****************************");

            if (list.Count > 0)
            {
                mapper.Options.Filter.Clear();
                mapper.Options.Filter.Add(new("Реквизит1", null));
                //mapper.Options.Filter.Add(new("Наименование", "Элемент 1.2"));
                mapper.Configure();

                foreach (var entity in mapper.Select())
                {
                    Console.WriteLine();
                    foreach (var property in entity)
                    {
                        Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                    }
                }
            }
        }
        [TestMethod] public void Test_Select_TablePart()
        {
            string metadataName = "Справочник.ПростойСправочник";

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            if (@object is not IAggregate aggregate)
            {
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object as ApplicationObject
            };
            options.Filter.Add(new FilterParameter("Код", 1));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var entity in list)
            {
                Console.WriteLine();
                foreach (var property in entity)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                }
            }

            Console.WriteLine();
            Console.WriteLine("****************************");

            if (list.Count > 0)
            {
                mapper.Options.Entity = aggregate.TableParts[0];
                mapper.Options.Filter.Clear();
                mapper.Options.Filter.Add(new("Ссылка", list[0]["Ссылка"]));
                mapper.Configure();

                foreach (var record in mapper.Select())
                {
                    Console.WriteLine();
                    foreach (var property in record)
                    {
                        Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                    }
                }
            }
        }
        [TestMethod] public void Test_Entity_Change_Table()
        {
            string metadataName = "Справочник.ПростойСправочник";

            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            if (@object is not ApplicationObject entity)
            {
                return;
            }

            EntityChangeTable changeTable = service.GetEntityChangeTable(entity);
            
            if (changeTable == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" does not have change table.");
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = changeTable
            };
            
            // TODO: отбор по узлу плана обмена !!!

            //options.Filter.Add(new FilterParameter("Ссылка", new Guid("e403d57f-fe02-11ec-9ccf-408d5c93cc8e")));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var record in list)
            {
                Console.WriteLine();
                foreach (var property in record)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                }

                if (record["УзелПланаОбмена"] is EntityRef node)
                {
                    ApplicationObject _node = service.GetApplicationObject(node.TypeCode);
                }
            }
        }
    }
}