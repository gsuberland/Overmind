namespace Overmind.Server.Exceptions
{
    class TaskNotFoundException : OvermindException
    {
        public string TaskName { get; set; }

        public TaskNotFoundException(string taskName) : base("The requested task was not found.")
        {
            StatusCode = 404;
            TaskName = taskName;
        }
    }
}
