using log4net;
using System.Text.Json;

namespace Overmind.Server.Config
{
    class OvermindConfig : IConfigElement
    {
        public string? InstanceName { get; set; }
        public WebServerConfig[]? Servers { get; set; }
        public TaskConfig[]? Tasks { get; set; }
        public string[]? CallbackDomains { get; set; }

        public bool Validate()
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            if (string.IsNullOrEmpty(InstanceName))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration is missing an instance name.");
                return false;
            }
            if (!(Servers?.Any() ?? false))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration does not include any server definitions.");
                return false;
            }
            if (!(Tasks?.Any() ?? false))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration does not include any task definitions.");
                return false;
            }
            foreach (var server in Servers)
            {
                if (!server.Validate())
                    return false;
            }
            foreach (var task in Tasks)
            {
                if (!task.Validate())
                    return false;
            }
            return true;
        }

        public static OvermindConfig? Load(string path)
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration being initialised from '{path}'");
            string fullConfigPath = Path.GetFullPath(path);
            _log.Debug($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Canonical configuration file path is '{fullConfigPath}'");
            if (!File.Exists(fullConfigPath))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] No config file found");
                return null;
            }
            try
            {
                OvermindConfig? config = null;
                using (var file = File.OpenRead(fullConfigPath))
                {
                    config = JsonSerializer.Deserialize<OvermindConfig>(file);
                }
                if (!(config?.Validate() ?? false))
                {
                    _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration validation error");
                    return null;
                }
                return config;
            }
            catch (Exception ex)
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Exception while loading configuration: {ex.ToString()}");
                return null;
            }
        }
    }
}
