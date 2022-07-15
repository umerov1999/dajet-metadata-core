using DaJet.Metadata;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaJet.Data.Mapping.Test
{
    [TestClass] public class UnitTest1
    {
        private const string IB_KEY = "dajet-metadata-ms";
        private readonly InfoBase _infoBase;
        private readonly MetadataCache _cache;
        private readonly MetadataService _service = new();
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True;Encrypt=False;";
        public UnitTest1()
        {
            _service.Add(new InfoBaseOptions()
            {
                Key = IB_KEY,
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SqlServer
            });

            if (!_service.TryGetInfoBase(IB_KEY, out _infoBase, out string error))
            {
                throw new InvalidOperationException($"Failed to open info base: {error}");
            }

            if (!_service.TryGetMetadataCache(IB_KEY, out _cache, out error))
            {
                throw new InvalidOperationException($"Failed to get metadata cache: {error}");
            }
        }
        private void ExecuteAndShow(DataMapperOptions options)
        {
            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return;
            }
            
            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            Console.WriteLine();
            Console.WriteLine(options.Entity.Name);

            if (list.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("������� 0 �������.");
            }

            foreach (var record in list)
            {
                Console.WriteLine();

                foreach (var property in record)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                }
            }
        }
        [TestMethod] public void Test_Select_Entity()
        {
            string metadataName = "����������.�����������������������������";

            MetadataObject @object = _cache.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = (@object as ApplicationObject)!
            };
            //options.Filter.Add(new FilterParameter("������", new Guid("e403d57f-fe02-11ec-9ccf-408d5c93cc8e")));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var entity in list)
            {
                Console.WriteLine();
                foreach (var property in entity)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                }
            }

            Console.WriteLine();
            Console.WriteLine("****************************");

            if (list.Count > 0)
            {
                mapper.Options.Filter.Clear();
                mapper.Options.Filter.Add(new("��������1", null));
                //mapper.Options.Filter.Add(new("������������", "������� 1.2"));
                mapper.Configure();

                foreach (var entity in mapper.Select())
                {
                    Console.WriteLine();
                    foreach (var property in entity)
                    {
                        Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                    }
                }
            }
        }
        [TestMethod] public void Test_Select_TablePart()
        {
            string metadataName = "����������.�����������������";

            MetadataObject @object = _cache.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            if (@object is not ITablePartOwner aggregate)
            {
                return;
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = (@object as ApplicationObject)!
            };
            options.Filter.Add(new FilterParameter("���", 1));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var entity in list)
            {
                Console.WriteLine();
                foreach (var property in entity)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                }
            }

            Console.WriteLine();
            Console.WriteLine("****************************");

            if (list.Count > 0)
            {
                mapper.Options.Entity = aggregate.TableParts[0];
                mapper.Options.Filter.Clear();
                mapper.Options.Filter.Add(new("������", list[0]["������"]));
                mapper.Configure();

                foreach (var record in mapper.Select())
                {
                    Console.WriteLine();
                    foreach (var property in record)
                    {
                        Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                    }
                }
            }
        }
        [TestMethod] public void Test_Select_InfoRegister()
        {
            string metadataName = "���������������.�������������������������������"; // ����������������������

            ApplicationObject @object = (_cache.GetMetadataObject(metadataName) as ApplicationObject)!;

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
            //options.Filter.Add(new FilterParameter("���������1", 20));
            options.Filter.Add(new FilterParameter("������", new DateTime(2022, 7, 2), ComparisonOperator.GreaterOrEqual));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var entity in list)
            {
                Console.WriteLine();
                foreach (var property in entity)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                }
            }
        }
        [TestMethod] public void Test_Select_AccumRegister()
        {
            string metadataName = "�����������������.������������������������"; // ������������������������

            ApplicationObject @object = (_cache.GetMetadataObject(metadataName) as ApplicationObject)!;

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
            //options.Filter.Add(new FilterParameter("����������", true));
            //options.Filter.Add(new FilterParameter("���������1", 20));
            //options.Filter.Add(new FilterParameter("�����������", 1)); // ������
            //options.Filter.Add(new FilterParameter("������", new DateTime(2022, 7, 2), ComparisonOperator.GreaterOrEqual));

            ExecuteAndShow(options);
        }
        [TestMethod] public void Test_Entity_Change_Table()
        {
            string metadataName = "����������.�����������������";

            MetadataObject @object = _cache.GetMetadataObject(metadataName);

            if (@object == null)
            {
                Console.WriteLine($"Metadata object \"{metadataName}\" is not found.");
                return;
            }

            if (@object is not ApplicationObject entity)
            {
                return;
            }

            EntityChangeTable changeTable = _cache.GetEntityChangeTable(entity);
            
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
            //options.Filter.Add(new FilterParameter("������", new Guid("e403d57f-fe02-11ec-9ccf-408d5c93cc8e")));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            foreach (var record in list)
            {
                Console.WriteLine();
                foreach (var property in record)
                {
                    Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                }

                if (record["���������������"] is EntityRef node)
                {
                    ApplicationObject _node = (_cache.GetMetadataObject(node.TypeCode) as ApplicationObject)!;
                }
            }
        }
        [TestMethod] public void Test_Filter_By_EntityRef()
        {
            string nodeCode = "N002";

            EntityChangeTable changeTable = GetChangeTable("����������.�����������������");

            EntityRef exchangeNode = GetExchangeNode("����������.����������", nodeCode);

            if (exchangeNode == EntityRef.Empty)
            {
                Console.WriteLine("���� ����� ������ �� ������.");
            }

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = changeTable
            };
            options.Filter.Add(new FilterParameter("���������������", exchangeNode));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count == 0)
            {
                Console.WriteLine($"����������� ��������� �� ���� {nodeCode} ���.");
            }

            foreach (var record in list)
            {
                Console.WriteLine();
                foreach (var property in record)
                {
                    if (property.Key == "���������������" && property.Value is EntityRef node)
                    {
                        string description = GetNodeDescription("����������.����������", node.Identity);
                        Console.WriteLine($"{property.Key} = {description} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                    }
                    else if (property.Key == "������" && property.Value is Guid identity)
                    {
                        string description = GetEntityDescription("����������.�����������������", identity);
                        Console.WriteLine($"{property.Key} = {description} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                    }
                    else
                    {
                        Console.WriteLine($"{property.Key} = {property.Value} [{(property.Value == null ? "������������" : property.Value.GetType())}]");
                    }
                }
            }
        }
        private EntityRef GetExchangeNode(string metadataName, string code)
        {
            ApplicationObject @object = (_cache.GetMetadataObject(metadataName) as ApplicationObject)!;

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            options.Filter.Add(new FilterParameter("���", code));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return EntityRef.Empty;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count > 0)
            {
                return new EntityRef(@object.TypeCode, (Guid)list[0]["������"]);
            }

            return EntityRef.Empty;
        }
        private EntityChangeTable GetChangeTable(string metadataName)
        {
            MetadataObject @object = _cache.GetMetadataObject(metadataName);

            if (@object is ApplicationObject entity)
            {
                return _cache.GetEntityChangeTable(entity);
            }

            return null!;
        }
        private string GetNodeDescription(string metadataName, Guid identity)
        {
            ApplicationObject @object = (_cache.GetMetadataObject(metadataName) as ApplicationObject)!;

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            options.Filter.Add(new FilterParameter("������", identity));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                Console.WriteLine($"Failed to get QueryExecutor: {error}");
                return error;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count > 0)
            {
                return (string)list[0]["������������"];
            }

            return "NOT FOUND";
        }
        private string GetEntityDescription(string metadataName, Guid identity)
        {
            ApplicationObject @object = (_cache.GetMetadataObject(metadataName) as ApplicationObject)!;

            DataMapperOptions options = new()
            {
                InfoBase = _infoBase,
                Entity = @object
            };
            options.Filter.Add(new FilterParameter("������", new EntityRef(@object.TypeCode, identity)));

            if (!_service.TryGetQueryExecutor(IB_KEY, out IQueryExecutor executor, out string error))
            {
                return error;
            }

            EntityDataMapper mapper = new(options, executor);

            var list = mapper.Select();

            if (list.Count > 0)
            {
                return (string)list[0]["������������"];
            }

            return "NOT FOUND";
        }
    }
}