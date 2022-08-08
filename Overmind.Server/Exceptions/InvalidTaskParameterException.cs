namespace Overmind.Server.Exceptions
{
    class InvalidTaskParameterException : OvermindException
    {
        public string ParameterName { get; set; }

        public InvalidTaskParameterException(string parameterName) : base("Task parameter validation failed.")
        {
            base.StatusCode = 422;
            ParameterName = parameterName;
        }
    }
}
