namespace DaJet.Metadata
{
    public sealed class InfoBaseOptions
    {
        public InfoBaseOptions(string key)
        {
            Key = key;
        }
        public string Key { get; } = string.Empty;
        ///<summary>Cache expiration period in seconds.</summary>
        public int Expiration { get; set; } = 600; // seconds
        public string ConnectionString { get; set; } = string.Empty;
        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.SqlServer;
    }
}