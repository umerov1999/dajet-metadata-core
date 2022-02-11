using DaJet.Metadata.Model;
using DaJet.Metadata.Parsers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaJet.Metadata.Core
{
    internal sealed class InfoBaseCache
    {
        private readonly DatabaseProvider _provider;
        private readonly string _connectionString;

        private Guid _root = Guid.Empty;

        private Dictionary<Guid, List<Guid>> metadata = new Dictionary<Guid, List<Guid>>();
        // ??? private Dictionary<Guid, Dictionary<Guid, MetaInfo>> metadata = new Dictionary<Guid, Dictionary<Guid, MetaInfo>>();
        private Dictionary<Guid, Dictionary<string, Guid>> _metadata = new Dictionary<Guid, Dictionary<string, Guid>>();

        private Dictionary<Guid, Guid> _references = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, MetadataObject> _cache;

        private DbNames _dataNames;
        private Dictionary<int, Guid> _codes;

        internal InfoBaseCache(DatabaseProvider provider, in string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
        }

        internal void Initialize(out InfoBase infoBase)
        {
            InitRootFile();

            InitInfoBase(out infoBase, out metadata);

            SetupMetadataCache();
        }
        private void InitRootFile()
        {
            using (ConfigFileReader reader = new ConfigFileReader(_provider, in _connectionString, ConfigTables.Config, ConfigFiles.Root))
            {
                _root = new RootFileParser().Parse(in reader);
            }
        }
        private void InitInfoBase(out InfoBase infoBase, out Dictionary<Guid, List<Guid>> metadata)
        {
            using (ConfigFileReader reader = new ConfigFileReader(_provider, in _connectionString, ConfigTables.Config, _root))
            {
                new InfoBaseParser().Parse(in reader, out infoBase, out metadata);
            }
        }
        private void SetupMetadataCache()
        {
            //TODO: add NamedDataTypeSet - to fill _references !?

            ParallelOptions options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.ForEach(metadata, options, SetupMetadataTypeCache);

            //SetupMetadataTypeCache(MetadataTypes.Catalog, in metadata);
            //SetupMetadataTypeCache(MetadataTypes.Document, in metadata);
            //SetupMetadataTypeCache(MetadataTypes.Enumeration, in metadata);
            //SetupMetadataTypeCache(MetadataTypes.Publication, in metadata);
            //SetupMetadataTypeCache(MetadataTypes.Characteristic, in metadata);
            //SetupMetadataTypeCache(MetadataTypes.InformationRegister, in metadata);
            //SetupMetadataTypeCache(MetadataTypes.AccumulationRegister, in metadata);
        }
        private void SetupMetadataTypeCache(Guid type, in Dictionary<Guid, List<Guid>> metadata)
        {
            if (!(metadata.TryGetValue(type, out List<Guid> list)))
            {
                return;
            }

            MetaInfoParser parser = new MetaInfoParser();

            foreach (Guid uuid in list)
            {
                if (uuid == Guid.Empty)
                {
                    continue;
                }

                using (ConfigFileReader reader = new ConfigFileReader(_provider, in _connectionString, ConfigTables.Config, uuid))
                {
                    MetaInfo info = parser.Parse(in reader, type);

                    if (info.Name == string.Empty)
                    {
                        continue; // accidentally: unsupported metadata object type
                    }

                    if (info.Uuid != Guid.Empty)
                    {
                        _references.Add(info.Uuid, uuid); // reference type uuid to metadata object uuid mapping
                    }

                    // metadata object name to uuid mapping
                    if (_metadata.TryGetValue(type, out Dictionary<string, Guid> names))
                    {
                        names.Add(info.Name, uuid);
                    }
                    else
                    {
                        _metadata.Add(type, new Dictionary<string, Guid>() { { info.Name, uuid } });
                    }
                }
            }
        }
        private void SetupMetadataTypeCache(KeyValuePair<Guid, List<Guid>> metadata)
        {
            Guid type = metadata.Key;

            //TODO: add NamedDataTypeSet - to fill _references !?

            if (!(metadata.Key == MetadataTypes.Catalog ||
                metadata.Key == MetadataTypes.Document ||
                metadata.Key == MetadataTypes.Enumeration ||
                metadata.Key == MetadataTypes.Publication ||
                metadata.Key == MetadataTypes.Characteristic ||
                metadata.Key == MetadataTypes.InformationRegister ||
                metadata.Key == MetadataTypes.AccumulationRegister))
            {
                return;
            }

            MetaInfoParser parser = new MetaInfoParser();

            foreach (Guid uuid in metadata.Value)
            {
                if (uuid == Guid.Empty)
                {
                    continue;
                }

                using (ConfigFileReader reader = new ConfigFileReader(_provider, in _connectionString, ConfigTables.Config, uuid))
                {
                    MetaInfo info = parser.Parse(in reader, type);

                    if (info.Name == string.Empty)
                    {
                        continue; // accidentally: unsupported metadata object type
                    }

                    if (info.Uuid != Guid.Empty)
                    {
                        _references.Add(info.Uuid, uuid); // reference type uuid to metadata object uuid mapping
                    }

                    // metadata object name to uuid mapping
                    if (_metadata.TryGetValue(type, out Dictionary<string, Guid> names))
                    {
                        names.Add(info.Name, uuid);
                    }
                    else
                    {
                        _metadata.Add(type, new Dictionary<string, Guid>() { { info.Name, uuid } });
                    }
                }
            }
        }
    }
}