using System.Text.Json;
using System.Text.Json.Serialization;

namespace Overmind.Server.Web
{
    static class JsonSettings
    {
        public static JsonSerializerOptions SerializerOptions
        {
            get => new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = 
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                }
            };
        }
    }
}