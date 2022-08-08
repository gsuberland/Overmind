using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using log4net;

namespace Overmind.Server.Config
{
    class TaskParameterConfig : IConfigElement
    {
        private static readonly Regex TaskNameValidationRegex = new Regex(@"^[A-Za-z0-9_]+$");

        public string? Name { get; set; }
        public string? ValidationRegex { get; set; }
        public string? ValidationBasePath { get; set; }

        [JsonPropertyName("ValidationRules")]
        private string[]? _validationFlags { get; set; } 

        [JsonIgnore]
        public ParameterValidationFlags ValidationFlags
        {
            get
            {
                // convert string list to flags value
                ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
                ParameterValidationFlags flags = 0;
                if (_validationFlags == null)
                    return 0;
                
                var remainingFlags = new List<string>(_validationFlags);
                foreach (var flagName in Enum.GetNames<ParameterValidationFlags>())
                {
                    if (_validationFlags?.Contains(flagName) ?? false)
                    {
                        flags |= Enum.Parse<ParameterValidationFlags>(flagName);
                        remainingFlags.Remove(flagName);
                    }
                }
                if (remainingFlags.Count > 0)
                {
                    _log.Warn($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Invalid validation flags in configuration for parameter '{Name}': {string.Join(' ', remainingFlags)}");
                }
                return flags;
            }
            set
            {
                // convert flags value to string list
                var flagNames = new List<string>();
                foreach (var flag in Enum.GetValues<ParameterValidationFlags>())
                {
                    if (value.HasFlag(flag))
                    {
                        string flagName = Enum.GetName<ParameterValidationFlags>(flag) ?? "";
                        if (flagName == "")
                        {
                            throw new InvalidOperationException($"Invalid flag value {(int)flag}");
                        }
                        flagNames.Add(flagName);
                    }
                }
            }
        }

        public bool Validate()
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            if (string.IsNullOrEmpty(Name))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration contains task parameter with no name");
                return false;
            }
            if (!TaskNameValidationRegex.IsMatch(Name))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration contains task parameter with invalid name '{Name}'");
                return false;
            }
            return true;
        }
    }
}
