namespace ByteSyncer.Domain.Exceptions
{
    public class EntityValidationException : Exception
    {
        public EntityValidationException(string? message, Dictionary<string, string[]> errors) : base(message)
        {
            Errors = errors;
        }

        public Dictionary<string, string[]> Errors { get; }
    }
}
