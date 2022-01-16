namespace DaJet.Metadata.Services
{
    public static class DbNamesFactory
    {
        public static string CreateDbName(DatabaseProvider provider, string name, int code)
        {
            if (provider == DatabaseProvider.SQLServer)
            {
                return $"_{name}{code}";
            }
            return $"_{name}{code}".ToLowerInvariant();
        }
    }
}