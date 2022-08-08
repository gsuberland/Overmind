using System.Collections.ObjectModel;

namespace Overmind.Messages
{
    public class TaskInstanceResponse : OvermindResponse
    {
        public Guid Id { get; protected set; }
        public string TaskName { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public DateTime? EndTime { get; protected set; }
        public int ProcessId { get; protected set; }
        public int? PlatformExitCode { get; protected set; }
        public OvermindTaskStatus Status { get; protected set; }
        public ReadOnlyDictionary<string, string> Parameters { get; protected set; }

        public TaskInstanceResponse(
            Guid id, 
            string taskName, 
            OvermindTaskStatus status, 
            DateTime startTime, 
            DateTime? endTime, 
            int processId, 
            int? platformExitCode, 
            ReadOnlyDictionary<string, string> parameters)
        {
            this.Id = id;
            this.TaskName = taskName;
            this.Status = status;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.ProcessId = processId;
            this.PlatformExitCode = platformExitCode;
            this.Parameters = parameters;
        }
    }
}