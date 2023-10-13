namespace ByteSyncer.Application.Utilities
{
    public static class SecurityStampUtilities
    {
        public static string GetSecurityStamp()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
