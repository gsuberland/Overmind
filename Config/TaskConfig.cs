using log4net;

namespace Overmind.Config
{
    class TaskConfig : IConfigElement
    {
        public string? Name { get; set; }
        public string? Executable { get; set; }
        public string[]? Arguments { get; set; }
        public TaskParameterConfig[]? Parameters { get; set; }

        public bool Validate()
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            if (string.IsNullOrEmpty(Name))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration contains task with missing name.");
                return false;
            }
            if (string.IsNullOrEmpty(Executable))
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Configuration contains task {Name} with missing executable.");
                return false;
            }
            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    if (!parameter.Validate())
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
