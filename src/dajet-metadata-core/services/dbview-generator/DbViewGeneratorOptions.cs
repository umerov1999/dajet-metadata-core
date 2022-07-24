﻿using DaJet.Data;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Services
{
    public sealed class DbViewGeneratorOptions
    {
        public string Schema { get; set; } = "dbo";
        public bool CodifyViewNames { get; set; } = false; // shorten view names for PostgreSql
        public string OutputFile { get; set; } = string.Empty;
        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.SqlServer;
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
            if (values.TryGetValue(nameof(DbViewGeneratorOptions.DatabaseProvider), out string DatabaseProvider)
                && !string.IsNullOrWhiteSpace(DatabaseProvider)
                && Enum.TryParse(DatabaseProvider, out DatabaseProvider provider))
            {
                options.DatabaseProvider = provider;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.ConnectionString), out string ConnectionString)
                && !string.IsNullOrWhiteSpace(ConnectionString))
            {
                options.ConnectionString = ConnectionString ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.Schema), out string Schema)
                && !string.IsNullOrWhiteSpace(Schema))
            {
                options.Schema = Schema ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.OutputFile), out string OutputFile)
                && !string.IsNullOrWhiteSpace(OutputFile))
            {
                options.OutputFile = OutputFile ?? string.Empty;
            }

            if (values.TryGetValue(nameof(DbViewGeneratorOptions.CodifyViewNames), out string CodifyViewNames)
                && !string.IsNullOrWhiteSpace(CodifyViewNames))
            {
                options.CodifyViewNames = (CodifyViewNames == "true");
            }
        }
    }
}