using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaJet.Metadata.Test
{
    [TestClass] public class Test_MetadataService
    {
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=test_node_1;Integrated Security=True";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=test_node_2;Username=postgres;Password=postgres;";

        [TestMethod] public void MS_TryOpenInfoBase()
        {
            MetadataService service = new MetadataService();
            service.TryOpenInfoBase(DatabaseProvider.SQLServer, MS_CONNECTION_STRING, out InfoBase infoBase);
        }
        [TestMethod] public void PG_TryOpenInfoBase()
        {
            MetadataService service = new MetadataService();
            service.TryOpenInfoBase(DatabaseProvider.PostgreSQL, PG_CONNECTION_STRING, out InfoBase infoBase);
        }
    }
}