using DaJet.Data;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace DaJet.Metadata.Services
{
    public abstract class DbViewGenerator : IDbViewGenerator
    {
        public static IDbViewGenerator Create(DbViewGeneratorOptions options)
        {
            if(options == null) throw new ArgumentNullException(nameof(options));

            if (!Enum.TryParse(options.DatabaseProvider, out DatabaseProvider provider))
            {
                throw new ArgumentException($"Unsupported database provider: [{options.DatabaseProvider}].");
            }

            if (provider == DatabaseProvider.SqlServer)
            {
                return new MsDbViewGenerator(options);
            }
            else if (provider == DatabaseProvider.PostgreSql)
            {
                return new PgDbViewGenerator(options);
            }

            throw new InvalidOperationException($"Unsupported database provider: [{options.DatabaseProvider}].");
        }

        protected readonly IQueryExecutor _executor;
        protected readonly DbViewGeneratorOptions _options;
        public DbViewGenerator(DbViewGeneratorOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (!Enum.TryParse(_options.DatabaseProvider, out DatabaseProvider provider))
            {
                throw new ArgumentException($"Unsupported database provider: [{_options.DatabaseProvider}].");
            }

            _executor = QueryExecutor.Create(provider, _options.ConnectionString);
        }

        #region "ABSTRACT MEMBERS"
        protected abstract string DEFAULT_SCHEMA_NAME { get; }
        protected abstract string DROP_VIEW_SCRIPT { get; }
        protected abstract string SELECT_VIEWS_SCRIPT { get; }
        protected abstract string DROP_SCHEMA_SCRIPT { get; }
        protected abstract string SCHEMA_EXISTS_SCRIPT { get; }
        protected abstract string CREATE_SCHEMA_SCRIPT { get; }
        protected abstract string FormatViewName(string viewName);
        public abstract string GenerateViewScript(in ApplicationObject metadata);
        public abstract string GenerateEnumViewScript(in Enumeration enumeration);
        #endregion

        #region "INTERFACE IMPLEMENTATION"

        public bool SchemaExists(string name)
        {
            string script = string.Format(SCHEMA_EXISTS_SCRIPT, name);

            return (_executor.ExecuteScalar<int>(in script, 10) == 1);
        }
        public void CreateSchema(string name)
        {
            string script = string.Format(CREATE_SCHEMA_SCRIPT, name);

            _executor.ExecuteNonQuery(in script, 10);
        }
        public void DropSchema(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            string script = string.Format(DROP_SCHEMA_SCRIPT, name);

            _executor.ExecuteNonQuery(in script, 10);
        }
        public bool TryCreateSchemaIfNotExists(out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(_options.Schema))
            {
                _options.Schema = DEFAULT_SCHEMA_NAME;
            }

            if (_options.Schema == DEFAULT_SCHEMA_NAME)
            {
                return true;
            }

            try
            {
                if (!SchemaExists(_options.Schema))
                {
                    CreateSchema(_options.Schema);
                }
            }
            catch (Exception exception)
            {
                error = $"Failed to create schema [{_options.Schema}]: {ExceptionHelper.GetErrorMessage(exception)}";
            }

            return string.IsNullOrEmpty(error);
        }

        public bool TryCreateView(in ApplicationObject metadata, out string error)
        {
            error = string.Empty;

            try
            {
                string viewName = Configurator.CreateViewName(metadata, _options.CodifyViewNames);

                List<string> scripts = new()
                {
                    string.Format(DROP_VIEW_SCRIPT, FormatViewName(viewName))
                };

                if (metadata is Enumeration enumeration)
                {
                    scripts.Add(GenerateEnumViewScript(enumeration));
                }
                else
                {
                    scripts.Add(GenerateViewScript(metadata));

                    if (metadata is ITablePartOwner owner)
                    {
                        foreach (TablePart table in owner.TableParts)
                        {
                            viewName = Configurator.CreateViewName(table, _options.CodifyViewNames);
                            scripts.Add(string.Format(DROP_VIEW_SCRIPT, FormatViewName(viewName)));
                            scripts.Add(GenerateViewScript(table));
                        }
                    }
                }

                _executor.TxExecuteNonQuery(scripts, 10);
            }
            catch (Exception exception)
            {
                error = $"[{metadata.Name}] [{metadata.TableName}] {ExceptionHelper.GetErrorMessage(exception)}";
            }

            return string.IsNullOrEmpty(error);
        }
        public bool TryCreateViews(in MetadataCache cache, out int result, out List<string> errors)
        {
            result = 0;
            errors = new();
            
            foreach (string typeName in _options.MetadataTypes)
            {
                Guid type = MetadataTypes.ResolveName(typeName);

                if (type == Guid.Empty)
                {
                    errors.Add($"Metadata type [{typeName}] is not supported.");
                    continue;
                }

                foreach (ApplicationObject metadata in cache.GetMetadataObjects(type))
                {
                    if (TryCreateView(metadata, out string error))
                    {
                        result++;
                    }
                    else
                    {
                        errors.Add(error);
                    }
                }
            }

            return (errors.Count == 0);
        }
        
        public int DropViews()
        {
            int result = 0;
            int VIEW_NAME = 1;

            string select = string.Format(SELECT_VIEWS_SCRIPT, _options.Schema);

            foreach (IDataReader reader in _executor.ExecuteReader(select, 30))
            {
                if (reader.IsDBNull(VIEW_NAME))
                {
                    continue;
                }

                string viewName = FormatViewName(reader.GetString(VIEW_NAME));

                string script = string.Format(DROP_VIEW_SCRIPT, viewName);

                _executor.ExecuteNonQuery(in script, 10);

                result++;
            }

            return result;
        }
        public void DropView(in ApplicationObject metadata)
        {
            string viewName = Configurator.CreateViewName(metadata, _options.CodifyViewNames);

            string script = string.Format(DROP_VIEW_SCRIPT, FormatViewName(viewName));
            
            _executor.ExecuteNonQuery(script, 10);
        }

        public bool TryScriptViews(in MetadataCache cache, out int result, out List<string> errors)
        {
            result = 0;
            errors = new();

            using (StreamWriter writer = new(_options.OutputFile, false, Encoding.UTF8))
            {
                foreach (string typeName in _options.MetadataTypes)
                {
                    Guid type = MetadataTypes.ResolveName(typeName);

                    if (type == Guid.Empty)
                    {
                        errors.Add($"Metadata type [{typeName}] is not supported.");
                        continue;
                    }

                    foreach (ApplicationObject metadata in cache.GetMetadataObjects(type))
                    {
                        if (TryScriptView(in writer, in metadata, out string error))
                        {
                            result++;
                        }
                        else
                        {
                            errors.Add(error);
                        }
                    }
                }
            }

            return (errors.Count == 0);
        }
        public bool TryScriptView(in StreamWriter writer, in ApplicationObject metadata, out string error)
        {
            error = string.Empty;

            string viewName = Configurator.CreateViewName(in metadata, _options.CodifyViewNames);

            try
            {
                writer.WriteLine(string.Format(DROP_VIEW_SCRIPT, FormatViewName(viewName)));
                writer.WriteLine("GO");

                if (metadata is Enumeration enumeration)
                {
                    writer.WriteLine(GenerateEnumViewScript(enumeration));
                }
                else
                {
                    writer.WriteLine(GenerateViewScript(metadata));

                    if (metadata is ITablePartOwner owner)
                    {
                        foreach (TablePart table in owner.TableParts)
                        {
                            viewName = Configurator.CreateViewName(table, _options.CodifyViewNames);
                            writer.WriteLine(string.Format(DROP_VIEW_SCRIPT, FormatViewName(viewName)));
                            writer.WriteLine(GenerateViewScript(table));
                        }
                    }
                }
                writer.WriteLine("GO");
            }
            catch (Exception exception)
            {
                error = $"[{metadata.Name}] [{metadata.TableName}] {ExceptionHelper.GetErrorMessage(exception)}";
            }

            return string.IsNullOrEmpty(error);
        }

        #endregion
    }
}