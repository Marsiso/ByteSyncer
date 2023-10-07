namespace ByteSyncer.Application.Options
{
    public class PasswordProtectorOptions
    {
        public string? Pepper { get; set; }
        public int WorkFactor { get; set; } = 10;
    }
}
