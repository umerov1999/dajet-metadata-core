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
        private void ExecuteAndShow(DataMapperOptions options)
        {
            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            Console.WriteLine();
            Console.WriteLine(options.Entity.Name);

            if (list.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Выбрано 0 записей.");
            }

            foreach (var record in list)
            {
                Console.WriteLine();

                foreach (var property in record)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                }
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
        [TestMethod] public void Test_Select_InfoRegister()
        {
            string metadataName = "РегистрСведений.ПериодическийМногоРегистраторов"; // ОбычныйРегистрСведений

            ApplicationObject @object = (service.GetMetadataObject(metadataName) as ApplicationObject)!;

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            //options.Filter.Add(new FilterParameter("Измерение1", 20));
            options.Filter.Add(new FilterParameter("Период", new DateTime(2022, 7, 2), ComparisonOperator.GreaterOrEqual));

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
        }
        [TestMethod] public void Test_Select_AccumRegister()
        {
            string metadataName = "РегистрНакопления.РегистрНакопленияОстатки"; // РегистрНакопленияОбороты

            ApplicationObject @object = (service.GetMetadataObject(metadataName) as ApplicationObject)!;

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            //options.Filter.Add(new FilterParameter("Активность", true));
            //options.Filter.Add(new FilterParameter("Измерение1", 20));
            //options.Filter.Add(new FilterParameter("ВидДвижения", 1)); // Расход
            //options.Filter.Add(new FilterParameter("Период", new DateTime(2022, 7, 2), ComparisonOperator.GreaterOrEqual));

            ExecuteAndShow(options);
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
        [TestMethod] public void Test_Filter_By_EntityRef()
        {
            string nodeCode = "N002";

            EntityChangeTable changeTable = GetChangeTable("Справочник.ПростойСправочник");

            EntityRef exchangeNode = GetExchangeNode("ПланОбмена.ПланОбмена", nodeCode);

            if (exchangeNode == EntityRef.Empty)
            {
                Console.WriteLine("Узел плана обмена не найден.");
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = changeTable
            };
            options.Filter.Add(new FilterParameter("УзелПланаОбмена", exchangeNode));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count == 0)
            {
                Console.WriteLine($"Регистрации изменений по узлу {nodeCode} нет.");
            }

            foreach (var record in list)
            {
                Console.WriteLine();
                foreach (var property in record)
                {
                    if (property.Key == "УзелПланаОбмена" && property.Value is EntityRef node)
                    {
                        string description = GetNodeDescription("ПланОбмена.ПланОбмена", node.Identity);
                        Console.WriteLine($"{property.Key} = {description} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                    }
                    else if (property.Key == "Ссылка" && property.Value is Guid identity)
                    {
                        string description = GetEntityDescription("Справочник.ПростойСправочник", identity);
                        Console.WriteLine($"{property.Key} = {description} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                    }
                    else
                    {
                        Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "Неопределено" : property.Value.GetType())}]");
                    }
                }
            }
        }
        private EntityRef GetExchangeNode(string metadataName, string code)
        {
            ApplicationObject @object = (service.GetMetadataObject(metadataName) as ApplicationObject)!;

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            options.Filter.Add(new FilterParameter("Код", code));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count > 0)
            {
                return new EntityRef(@object.TypeCode, (Guid)list[0]["Ссылка"]);
            }

            return EntityRef.Empty;
        }
        private EntityChangeTable GetChangeTable(string metadataName)
        {
            MetadataObject @object = service.GetMetadataObject(metadataName);

            if (@object is ApplicationObject entity)
            {
                return service.GetEntityChangeTable(entity);
            }

            return null!;
        }
        private string GetNodeDescription(string metadataName, Guid identity)
        {
            ApplicationObject @object = (service.GetMetadataObject(metadataName) as ApplicationObject)!;

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            options.Filter.Add(new FilterParameter("Ссылка", identity));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count > 0)
            {
                return (string)list[0]["Наименование"];
            }

            return "NOT FOUND";
        }
        private string GetEntityDescription(string metadataName, Guid identity)
        {
            ApplicationObject @object = (service.GetMetadataObject(metadataName) as ApplicationObject)!;

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            options.Filter.Add(new FilterParameter("Ссылка", new EntityRef(@object.TypeCode, identity)));

            IQueryExecutor executor = service.CreateQueryExecutor();

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count > 0)
            {
                return (string)list[0]["Наименование"];
            }

            return "NOT FOUND";
        }
    }
}