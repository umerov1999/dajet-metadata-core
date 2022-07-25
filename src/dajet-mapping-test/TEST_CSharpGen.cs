using DaJet.CSharp.Generator;
using DaJet.Data;
using DaJet.Metadata;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace DaJet.CSharp.Test
{
    [TestClass] public class TEST_CSharpGen
    {
        private const string IB_KEY = "dajet-metadata-ms";
        private readonly InfoBase _infoBase;
        private readonly MetadataCache _cache;
        private readonly MetadataService _service = new();
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-metadata-ms;Integrated Security=True;Encrypt=False;";
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=trade_11_2_3_159_demo;Integrated Security=True;Encrypt=False;";
        //private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=accounting_3_0_72_72_demo;Integrated Security=True;Encrypt=False;";
        public TEST_CSharpGen()
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
        [TestMethod] public void GenerateCodeToFile()
        {
            EntityModelGeneratorOptions options = new()
            {
                Version = _infoBase.AppConfigVersion,
                OutputFile = "C:\\temp\\cs-code\\" + _infoBase.Name + ".cs"
            };

            EntityModelGenerator generator = new(options, _cache);

            Assembly assembly = generator.Generate(out List<string> errors);

            foreach (string error in errors)
            {
                Console.WriteLine(error);
            }

            if (assembly is not null)
            {
                Console.WriteLine(assembly.FullName);
            }
        }
    }
}