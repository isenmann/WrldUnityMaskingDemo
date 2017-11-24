using System.Text.RegularExpressions;

namespace Wrld.Scripts.Utilities
{
    public static class APIKeyHelpers
    {
        public static void CacheAPIKey(string apiKey)
        {
            if (AppearsValid(apiKey))
            {
                ms_cachedApiKey = apiKey;
            }
        }

        public static string GetCachedAPIKey()
        {
            return ms_cachedApiKey;
        }

        public static bool AppearsValid(string apiKey)
        {
            return Regex.IsMatch(apiKey, "^[a-f0-9]{32}$");
        }

        private static string ms_cachedApiKey = null;
    }
}

