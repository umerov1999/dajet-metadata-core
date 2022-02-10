using DaJet.Metadata.Model;

namespace DaJet.Metadata.Core
{
    public sealed class MetadataService
    {
        private InfoBaseCache _cache;

        public bool TryOpenInfoBase(DatabaseProvider provider, in string connectionString, out InfoBase infoBase)
        {
            _cache = new InfoBaseCache(provider, in connectionString);

            _cache.Initialize(out infoBase);

            return true;
        }
    }
}