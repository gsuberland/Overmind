namespace Overmind.Tasks
{
    class InvalidTaskParameterException : Exception
    {
        public string ParameterName { get; set; }

        public InvalidTaskParameterException(string parameterName)
        {
            ParameterName = parameterName;
        }
    }
}
