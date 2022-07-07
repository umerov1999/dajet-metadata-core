namespace DaJet.Metadata.Model
{
    public sealed class EntityChangeTable : ApplicationObject
    {
        internal EntityChangeTable(ApplicationObject entity)
        {
            Entity = entity;
        }
        public ApplicationObject Entity { get; }
    }
}