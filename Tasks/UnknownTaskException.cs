namespace Overmind.Tasks
{
    class UnknownTaskException : Exception
    {
        public string TaskName { get; set; }

        public UnknownTaskException(string taskName)
        {
            TaskName = taskName;
        }
    }
}
