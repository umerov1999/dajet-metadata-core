using System.Collections.Generic;

namespace DaJet.Metadata.Services
{
    public sealed class DbViewGeneratorOptions
    {
        public string Schema { get; set; } = "dbo";
        public bool EncodeNames { get; set; } = false;
        public string OutputFile { get; set; } = string.Empty;
        public string DatabaseProvider { get; set; } = string.Empty; // { SqlServer, PostgreSql }
        public string ConnectionString { get; set; } = string.Empty;
        public List<string> MetadataTypes { get; set; } = new()
        {
            "Документ",
            "Справочник",
            "ПланОбмена",
            "Перечисление",
            "РегистрСведений",
            "РегистрНакопления",
            "ПланВидовХарактеристик"
            //TODO: not supported "Constant" (Константа)
            //TODO: not supported "Account" (ПланСчетов)
            //TODO: not supported "AccountingRegister" (РегистрБухгалтерии)
        };
        public static void Configure(in DbViewGeneratorOptions options, Dictionary<string, string> values)
        {
            if (values.TryGetValue(nameof(DbViewGeneratorOptions.DatabaseProvider), out string DatabaseProvider))
            {
                options.DatabaseProvider = DatabaseProvider ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.ConnectionString), out string ConnectionString))
            {
                options.ConnectionString = ConnectionString ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.Schema), out string Schema))
            {
                options.Schema = Schema ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.OutputFile), out string OutputFile))
            {
                options.OutputFile = OutputFile ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.EncodeViewNames), out string EncodeViewNames))
            {
                options.EncodeViewNames = (EncodeViewNames == "true");
            }
        }
    }
}