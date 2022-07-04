namespace DaJet.Metadata
{
    public sealed class MetadataServiceOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.SQLServer;
    }
}