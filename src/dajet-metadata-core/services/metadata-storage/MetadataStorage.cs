using DaJet.Data;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaJet.Metadata.Services
{
    public sealed class MetadataStorageOptions
    {

    }
    public interface IMetadataStorage
    {

    }
    public sealed class MetadataStorage : IMetadataStorage
    {
        private readonly IMetadataService _service;
        private readonly MetadataStorageOptions _options;
        public MetadataStorage(MetadataStorageOptions options, IMetadataService service)
        {
            _options = options;
            _service = service;
        }
        public bool TrySave(InfoBaseOptions source, InfoBaseOptions target, out string error)
        {
            error = string.Empty;

            if (!_service.TryGetMetadataCache(source.Key, out MetadataCache cache, out error))
            {
                return false;
            }

            IQueryExecutor executor = QueryExecutor.Create(target.DatabaseProvider, target.ConnectionString);

            if (executor == null)
            {
                error = $"Unsupported database provider: {{{target.DatabaseProvider}}}.";
                return false;
            }

            if (!TryCreateDatabase(in executor, target, out error))
            {
                return false;
            }

            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            try
            {
                SaveInfoBase(cache.InfoBase);

                foreach (Guid type in MetadataTypes.AllSupportedTypes)
                {
                    ParallelLoopResult result = Parallel.ForEach(cache.GetMetadataObjects(type), options, SaveMetadataObject);
                }
            }
            catch (Exception exception)
            {
                error = ExceptionHelper.GetErrorMessage(exception);
            }

            return string.IsNullOrWhiteSpace(error);
        }

        #region "DATABASE SCHEMA"

        //private const string SCHEMA_DBO = "dbo"; // default SQL Server schema
        //private const string SELECT_SCHEMA_SCRIPT = "SELECT schema_id, name FROM sys.schemas WHERE name = N'{0}';";
        //private const string CREATE_SCHEMA_SCRIPT = "CREATE SCHEMA {0};";
        //private const string DROP_SCHEMA_SCRIPT = "DROP SCHEMA {0};";
        //public bool SchemaExists(string name)
        //{
        //    string script = string.Format(SELECT_SCHEMA_SCRIPT, name);

        //    int schema_id = _executor.ExecuteScalar<int>(in script, 10);

        //    return (schema_id > 0);
        //}
        //public void CreateSchema(string name)
        //{
        //    string script = string.Format(CREATE_SCHEMA_SCRIPT, name);

        //    _executor.ExecuteNonQuery(in script, 10);
        //}
        //public void DropSchema(string name)
        //{
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        throw new ArgumentNullException(nameof(name));
        //    }

        //    string script = string.Format(DROP_SCHEMA_SCRIPT, name);

        //    _executor.ExecuteNonQuery(in script, 10);
        //}
        //private bool TryCreateSchemaIfNotExists(out string error)
        //{
        //    error = string.Empty;

        //    if (string.IsNullOrWhiteSpace(_options.Schema))
        //    {
        //        _options.Schema = SCHEMA_DBO;
        //    }

        //    if (_options.Schema == SCHEMA_DBO)
        //    {
        //        return true;
        //    }

        //    try
        //    {
        //        if (!SchemaExists(_options.Schema))
        //        {
        //            CreateSchema(_options.Schema);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        error = $"Failed to create schema [{_options.Schema}]: {ExceptionHelper.GetErrorText(exception)}";
        //    }

        //    return string.IsNullOrEmpty(error);
        //}

        #endregion

        #region "CREATE DATABASE"

        private bool TryCreateDatabase(in IQueryExecutor executor, InfoBaseOptions target, out string error)
        {
            error = string.Empty;

            List<string> scripts = PrepareCreateDatabaseScripts(executor.GetDatabaseName());

            try
            {
                executor.TxExecuteNonQuery(in scripts, 60);
            }
            catch (Exception exception)
            {
                error = ExceptionHelper.GetErrorMessage(exception);
            }

            return string.IsNullOrWhiteSpace(error);
        }
        private List<string> PrepareCreateDatabaseScripts(string databaseName)
        {
            List<string> scripts = new();

            scripts.Add("USE master;"); // ?

            string script = "IF DB_ID(N'{DATABASE_NAME}') IS NULL CREATE DATABASE {DATABASE_NAME};";
            script = script.Replace("{DATABASE_NAME}", databaseName);

            scripts.Add(script);

            //TODO

            return scripts;
        }

        #endregion

        private void SaveInfoBase(InfoBase infoBase)
        {
            //TODO
        }
        private void SaveMetadataObject(MetadataObject @object)
        {
            //TODO
        }
    }
}