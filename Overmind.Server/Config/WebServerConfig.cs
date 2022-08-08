using log4net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Overmind.Server.Config
{
    class WebServerConfig : IConfigElement
    {
        public string? Host { get; set; }
        public bool? Secure { get; set; }

        [JsonConverter(typeof(DeserializeOnlyConverter<string>))]
        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
        public string? Username { get; set; }
        
        [JsonConverter(typeof(DeserializeOnlyConverter<string>))]
        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
        public string? Password { get; set; }

        [JsonNumberHandling(JsonNumberHandling.Strict)]
        public int Port { get; set; }

        public bool Validate()
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            if (Host == "+")
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration contains web server entry with host '+' which is disallowed.");
                return false;
            }
            if (Port <= 0 || Port > 65535)
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration contains web server entry with invalid port {Port}");
                return false;
            }
            return true;
        }
    }
}
