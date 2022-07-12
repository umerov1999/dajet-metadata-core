using DaJet.Data;
using DaJet.Data.PostgreSql;
using DaJet.Data.SqlServer;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using DaJet.Metadata.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DaJet.Metadata
{
    public interface IMetadataService : IDisposable
    {
        List<InfoBaseOptions> Options { get; }
        void Add(InfoBaseOptions options);
        void Remove(string key);

        bool TryGetInfoBase(string key, out InfoBase infoBase, out string error);
        bool TryGetMetadataCache(string key, out MetadataCache cache, out string error);
        bool TryGetQueryExecutor(string key, out IQueryExecutor executor, out string error);
    }
    public sealed class MetadataService : IMetadataService
    {
        private const string ERROR_CASH_ENTRY_KEY_IS_NOT_FOUND = "Cache entry key [{0}] is not found.";
        private const string ERROR_UNSUPPORTED_DATABASE_PROVIDER = "Unsupported database provider: [{0}].";

        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

        public List<InfoBaseOptions> Options
        {
            get
            {
                List<InfoBaseOptions> list = new();

                foreach (var item in _cache)
                {
                    list.Add(item.Value.Options);
                }

                return list;
            }
        }
        public void Add(InfoBaseOptions options)
        {
            _ = _cache.TryAdd(options.Key, new CacheEntry(options));
        }
        public void Remove(string key)
        {
            if (_cache.TryRemove(key, out CacheEntry entry))
            {
                entry?.Dispose();
            }
        }
        
        public bool TryGetInfoBase(string key, out InfoBase infoBase, out string error)
        {
            if (!_cache.TryGetValue(key, out CacheEntry entry))
            {
                infoBase = null;
                error = string.Format(ERROR_CASH_ENTRY_KEY_IS_NOT_FOUND, key);
                return false;
            }

            Guid root;
            error = string.Empty;

            try
            {
                using (ConfigFileReader reader = new(
                    entry.Options.DatabaseProvider, entry.Options.ConnectionString, ConfigTables.Config, ConfigFiles.Root))
                {
                    root = new RootFileParser().Parse(in reader);
                }

                using (ConfigFileReader reader = new(
                    entry.Options.DatabaseProvider, entry.Options.ConnectionString, ConfigTables.Config, root))
                {
                    new InfoBaseParser().Parse(in reader, out infoBase);
                }
            }
            catch (Exception exception)
            {
                infoBase = null;
                error = ExceptionHelper.GetErrorMessage(exception);
            }

            return (infoBase != null);
        }
        public bool TryGetMetadataCache(string key, out MetadataCache cache, out string error)
        {
            if (!_cache.TryGetValue(key, out CacheEntry entry))
            {
                cache = null;
                error = string.Format(ERROR_CASH_ENTRY_KEY_IS_NOT_FOUND, key);
                return false;
            }

            error = string.Empty;
            cache = entry.Value;

            if (cache != null && !entry.IsExpired)
            {
                return true;
            }

            using (entry.UpdateLock())
            {
                cache = entry.Value;

                if (cache != null && !entry.IsExpired)
                {
                    return true;
                }

                cache = new MetadataCache(entry.Options.DatabaseProvider, entry.Options.ConnectionString);

                try
                {
                    cache.Initialize();
                }
                catch (Exception exception)
                {
                    cache = null;
                    error = ExceptionHelper.GetErrorMessage(exception);
                    return false;
                }

                // Assignment of the Value property internally refreshes the last update timestamp

                entry.Value = cache;
            }

            return (cache != null);
        }
        public bool TryGetQueryExecutor(string key, out IQueryExecutor executor, out string error)
        {
            error = string.Empty;

            if (!_cache.TryGetValue(key, out CacheEntry entry))
            {
                executor = null;

                error = string.Format(ERROR_CASH_ENTRY_KEY_IS_NOT_FOUND, key);

                return false;
            }

            if (entry.Options.DatabaseProvider == DatabaseProvider.SqlServer)
            {
                executor = new MsQueryExecutor(entry.Options.ConnectionString);
            }
            else if (entry.Options.DatabaseProvider == DatabaseProvider.PostgreSql)
            {
                executor = new PgQueryExecutor(entry.Options.ConnectionString);
            }
            else
            {
                executor = null;

                error = string.Format(ERROR_UNSUPPORTED_DATABASE_PROVIDER, entry.Options.DatabaseProvider);
            }

            return (executor != null);
        }

        public void Dispose()
        {
            foreach (CacheEntry entry in _cache.Values)
            {
                entry.Dispose();
            }
            _cache.Clear();
        }


        // Удалить всё внизу !!!

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
        public MetadataItem GetMetadataItem(Guid uuid)
        {
            return _cache.GetMetadataItem(uuid);
        }

        public string GetMainTableName(Guid uuid)
        {
            if (!_cache.TryGetDbName(uuid, out DbName entry))
            {
                return string.Empty;
            }

            if (_options.DatabaseProvider == DatabaseProvider.PostgreSql)
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

            if (_options.DatabaseProvider == DatabaseProvider.PostgreSql)
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

        // **************************************

        
        
        
    }
}