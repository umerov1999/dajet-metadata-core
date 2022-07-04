using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace DaJet.Data.PostgreSql
{
    public sealed class PgQueryExecutor : QueryExecutor
    {
        public PgQueryExecutor(in string connectionString) : base(connectionString)
        {
            // do nothing
        }
        protected override DbConnection GetDbConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
        protected override void ConfigureQueryParameters(in DbCommand command, in Dictionary<string, object> parameters)
        {
            if (command is not NpgsqlCommand _command)
            {
                throw new InvalidOperationException(nameof(command));
            }

            foreach (var parameter in parameters)
            {
                _command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
        }
    }
}