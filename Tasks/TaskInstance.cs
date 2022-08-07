using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using log4net;
using Overmind.Config;

namespace Overmind.Tasks
{
    class TaskInstance
    {
        private Process _process;

        [JsonIgnore]
        public TaskConfig Config { get; private set; }

        public Guid Id { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public int ProcessId { get { return _process.Id; } }
        public int? PlatformExitCode { get; private set; }
        public TaskStatus Status { get; private set; }

        public TaskInstance(TaskConfig config, Dictionary<string, string> parameters)
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

            Config = config;

            // validate parameter names provided from the request
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                if (!config.Parameters.Any(p => p.Name == parameter.Key))
                {
                    _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Invalid parameter '{parameter.Key}'");
                    throw new InvalidTaskParameterException(parameter.Key);
                }
            }

            // validate parameter values via regex where specified
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                var parameterConfig = config.Parameters.First(p => p.Name == parameter.Key);
                if (!string.IsNullOrEmpty(parameterConfig.ValidationRegex))
                {
                    var regex = new Regex(parameterConfig.ValidationRegex);
                    if (!regex.IsMatch(parameter.Value))
                    {
                        throw new InvalidTaskParameterException(parameter.Key);
                    }
                }
            }

            // validate path parameters
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                var parameterConfig = config.Parameters.First(p => p.Name == parameter.Key);
                if (!string.IsNullOrEmpty(parameterConfig.ValidationBasePath))
                {
                    if (!PathUtils.IsSubPath(parameter.Value, parameterConfig.ValidationBasePath, false))
                    {
                        throw new InvalidTaskParameterException(parameter.Key);
                    }
                }
            }

            // do a find/replace on the parameters, targeting the executable path and args
            string[] parameterNames = parameters.Keys.Select(k => $"@@{k}@@").ToArray();
            string[] parameterValues = parameters.Values.ToArray();
            string executable = config.Executable.ReplaceMany(parameterNames, parameterValues);
            string[] arguments = config.Arguments.ReplaceMany(parameterNames, parameterValues).ToArray();

            // start the process
            var psi = new ProcessStartInfo
            {
                FileName = executable,
            };
            foreach (var argument in arguments)
            {
                psi.ArgumentList.Add(argument);
            }
            _process = new Process();
            _process.EnableRaisingEvents = true;
            _process.StartInfo = psi;
            _process.Exited += (object? sender, EventArgs args) => {
                EndTime = DateTime.UtcNow;
                PlatformExitCode = _process.ExitCode;
                Status = TaskStatus.Completed;
            };
            Id = Guid.NewGuid();
            StartTime = DateTime.UtcNow;
            try
            {
                _process.Start();
                _process.Refresh();
            }
            catch (Exception ex)
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Error starting task {config.Name}: {ex.ToString()}");
                Status = TaskStatus.Failed;
            }
            Status = TaskStatus.Running;
        }
    }
}
