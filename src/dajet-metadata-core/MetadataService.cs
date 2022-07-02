using DaJet.Metadata.Model;
using DaJet.Metadata.Services;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Core
{
    public sealed class MetadataService
    {
        private MetadataCache _cache;

        public void OpenInfoBase(DatabaseProvider provider, in string connectionString, out InfoBase infoBase)
        {
            _cache = new MetadataCache(provider, in connectionString);

            UpdateInfoBase(out infoBase);
        }
        public void UpdateInfoBase(out InfoBase infoBase)
        {
            _cache.Initialize(out infoBase);
        }
        public MetadataObject GetMetadataObject(in InfoBase infoBase, string metadataName)
        {
            if (infoBase == null)
            {
                throw new ArgumentNullException(nameof(infoBase));
            }

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
        public MetadataObject GetMetadataObject(in InfoBase infoBase, Guid type, Guid uuid)
        {
            if (infoBase == null)
            {
                throw new ArgumentNullException(nameof(infoBase));
            }

            return _cache.GetMetadataObjectCached(type, uuid);
        }
        public MetadataObject GetMetadataObjectByReference(in InfoBase infoBase, Guid reference)
        {
            if (infoBase == null)
            {
                throw new ArgumentNullException(nameof(infoBase));
            }

            if (!_cache.TryGetReferenceInfo(reference, out MetadataEntry info))
            {
                return null;
            }

            return _cache.GetMetadataObjectCached(info.MetadataType, info.MetadataUuid);
        }
        public void GetMetadataObject(in InfoBase infoBase, Guid type, Guid uuid, out MetadataObject metadata)
        {
            if (infoBase == null)
            {
                throw new ArgumentNullException(nameof(infoBase));
            }

            _cache.GetMetadataObject(type, uuid, out metadata);
        }

        public IEnumerable<MetadataObject> GetMetadataObjects(in InfoBase infoBase, Guid type)
        {
            if (infoBase == null)
            {
                throw new ArgumentNullException(nameof(infoBase));
            }

            return _cache.GetMetadataObjects(type);
        }

        public DbName GetChangeTableName(MetadataObject metadata)
        {
            return _cache.GetChngR(metadata.Uuid);
        }
    }
}