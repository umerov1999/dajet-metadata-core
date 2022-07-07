using DaJet.Data;
using DaJet.Data.PostgreSql;
using DaJet.Data.SqlServer;
using DaJet.Metadata.Model;
using DaJet.Metadata.Services;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Core
{
    public sealed class MetadataService
    {
        private MetadataCache _cache;
        private readonly MetadataServiceOptions _options = new();
        
        public MetadataService() { }
        public MetadataService(MetadataServiceOptions options)
        {
            Configure(options);
        }
        public void Configure(MetadataServiceOptions options)
        {
            _options.DatabaseProvider = options.DatabaseProvider;
            _options.ConnectionString = options.ConnectionString;
        }
        public void Configure(Dictionary<string,string> options)
        {
            if (options.TryGetValue(nameof(MetadataServiceOptions.DatabaseProvider), out string option1))
            {
                if (Enum.TryParse(option1, out DatabaseProvider provider))
                {
                    _options.DatabaseProvider = provider;
                }
            }

            if (options.TryGetValue(nameof(MetadataServiceOptions.ConnectionString), out string option2))
            {
                _options.ConnectionString = option2 ?? string.Empty;
            }
        }
        
        public IQueryExecutor CreateQueryExecutor()
        {
            if (_options.DatabaseProvider == DatabaseProvider.SQLServer)
            {
                return new MsQueryExecutor(_options.ConnectionString);
            }
            else if (_options.DatabaseProvider == DatabaseProvider.PostgreSQL)
            {
                return new PgQueryExecutor(_options.ConnectionString);
            }

            throw new InvalidOperationException($"Unsupported database provider: {_options.DatabaseProvider}");
        }
        
        public bool TryOpenInfoBase(out InfoBase infoBase, out string error)
        {
            infoBase = null;
            error = string.Empty;
            
            try
            {
                _cache = new MetadataCache(_options.DatabaseProvider, _options.ConnectionString);

                _cache.Initialize(out infoBase);
            }
            catch(Exception exception)
            {
                error = ExceptionHelper.GetErrorMessage(exception);
            }

            return string.IsNullOrEmpty(error);
        }
        public MetadataObject GetMetadataObject(string metadataName)
        {
            if (string.IsNullOrWhiteSpace(metadataName))
            {
                throw new ArgumentNullException(nameof(metadataName));
            }

            string[] names = metadataName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (names.Length < 2)
            {
                throw new FormatException(nameof(metadataName));
            }

            string typeName = names[0];
            string objectName = names[1];

            //string tablePartName = null;
            //if (names.Length == 3)
            //{
            //    tablePartName = names[2];
            //}

            return _cache.GetMetadataObjectCached(typeName, objectName);
        }
        public MetadataObject GetMetadataObject(Guid type, Guid uuid)
        {
            return _cache.GetMetadataObjectCached(type, uuid);
        }
        public IEnumerable<MetadataObject> GetMetadataObjects(Guid type)
        {
            return _cache.GetMetadataObjects(type);
        }
        public ApplicationObject GetApplicationObject(int typeCode)
        {
            return _cache.GetApplicationObject(typeCode);
        }
        public MetadataEntity GetMetadataEntity(Guid uuid)
        {
            return _cache.GetMetadataEntity(uuid);
        }

        public string GetMainTableName(Guid uuid)
        {
            if (!_cache.TryGetDbName(uuid, out DbName entry))
            {
                return string.Empty;
            }

            if (_options.DatabaseProvider == DatabaseProvider.PostgreSQL)
            {
                return $"_{entry.Name}{entry.Code}".ToLowerInvariant();
            }

            return $"_{entry.Name}{entry.Code}";
        }
        public string GetChangeTableName(Guid uuid)
        {
            if (!_cache.TryGetChngR(uuid, out DbName entry))
            {
                return string.Empty;
            }

            if (_options.DatabaseProvider == DatabaseProvider.PostgreSQL)
            {
                return $"_{entry.Name}{entry.Code}".ToLowerInvariant();
            }

            return $"_{entry.Name}{entry.Code}";
        }
        public string GetChangeTableName(MetadataObject metadata)
        {
            return GetChangeTableName(metadata.Uuid);
        }

        public Publication GetPublication(string name)
        {
            string metadataName = "ПланОбмена." + name;

            Publication publication = GetMetadataObject(metadataName) as Publication;

            PublicationDataMapper mapper = new(_cache);

            mapper.Select(in publication);

            return publication;
        }
        public EntityChangeTable GetEntityChangeTable(ApplicationObject entity)
        {
            return _cache.GetEntityChangeTable(entity);
        }
    }
}