namespace Overmind.Exceptions
{
    class InvalidRequestParameterException : OvermindException
    {
        public string ParameterName { get; set; }

        public InvalidRequestParameterException(string parameterName) : base("A parameter in the request was improperly formatted.")
        {
            base.StatusCode = 422;
            ParameterName = parameterName;
        }
    }
}
