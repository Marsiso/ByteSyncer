namespace ByteSyncer.Domain.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public string? EntityIdentifier { get; }
        public string? EntityName { get; }

        public EntityNotFoundException(string? entityIdentifier, string? entityName)
        {
            EntityIdentifier = entityIdentifier;
            EntityName = entityName;
        }
    }
}
