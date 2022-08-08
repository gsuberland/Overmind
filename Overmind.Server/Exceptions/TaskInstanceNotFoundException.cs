namespace Overmind.Server.Exceptions
{
    class TaskInstanceNotFoundException : OvermindException
    {
        public Guid TaskId { get; set; }

        public TaskInstanceNotFoundException(Guid taskId) : base("The requested task instance was not found.")
        {
            StatusCode = 404;
            TaskId = taskId;
        }
    }
}
