using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Overmind.Server.Web
{
    static class SecurityTokenManager
    {
        private static readonly ConcurrentDictionary<string, DateTime> _tokens = new ConcurrentDictionary<string, DateTime>();
        private static readonly object _syncRoot = new object();
        private static DateTime _lastCleanupTime = DateTime.UtcNow;

        public static bool Validate(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            bool doCleanup = false;
            lock (_syncRoot)
            {
                if (DateTime.UtcNow - _lastCleanupTime > TimeSpan.FromMinutes(1))
                {
                    // cleanup time!
                    _lastCleanupTime = DateTime.UtcNow;
                    doCleanup = true;
                }
            }

            if (doCleanup)
            {
                Task.Run(() =>
                {
                    var expiryTime = DateTime.UtcNow - TimeSpan.FromHours(1);
                    var expiredTokens = _tokens.Where(t => t.Value < expiryTime).Select(t => t.Key);
                    foreach (var expiredToken in expiredTokens)
                    {
                        _tokens.TryRemove(expiredToken, out _);
                    }
                });
            }

            DateTime expiry;
            if (!_tokens.TryGetValue(key, out expiry))
                return false;
            if (expiry < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                _tokens.TryRemove(key, out expiry);
                return false;
            }
            return true;
        }

        public static (string, DateTime) Create()
        {
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow;
            while (!_tokens.TryAdd(token, expiry))
            {
                token = Guid.NewGuid().ToString();
            }
            return (token, expiry);
        }
    }
}
