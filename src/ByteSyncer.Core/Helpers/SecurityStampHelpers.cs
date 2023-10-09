namespace ByteSyncer.Core.Helpers
{
    public static class SecurityStampHelpers
    {
        public static string GetSecurityStamp()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
