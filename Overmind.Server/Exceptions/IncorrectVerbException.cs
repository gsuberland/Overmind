namespace Overmind.Server.Exceptions
{
    class IncorrectVerbException : OvermindException
    {
        public IncorrectVerbException() : base("The HTTP verb (e.g. GET, POST) was incorrect for this request.")
        {
            base.StatusCode = 400;
        }
    }
}
