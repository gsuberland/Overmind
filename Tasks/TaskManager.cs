using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using log4net;
using Overmind.Config;

namespace Overmind.Tasks
{
    static class TaskManager
    {
        private static ConcurrentDictionary<Guid, TaskInstance> _instances = new ConcurrentDictionary<Guid, TaskInstance>();

        public static TaskInstance[] GetInstances()
        {
            return _instances.Values.ToArray();
        }

        public static TaskInstance? GetInstance(Guid id)
        {
            TaskInstance? task;
            if (_instances.TryGetValue(id, out task))
                return task;
            return null;
        }

        public static TaskInstance Start(string taskName, Dictionary<string, string> parameters)
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

            var taskConfig = Program.Config.Tasks.FirstOrDefault(t => string.Equals(t.Name, taskName, StringComparison.CurrentCulture));
            if (taskConfig == null)
            {
                _log.Error($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Unknown task '{taskName}'");
                throw new UnknownTaskException(taskName);
            }
            var task = new TaskInstance(taskConfig, parameters);
            _instances.TryAdd(task.Id, task);
            return task;
        }
    }
}
