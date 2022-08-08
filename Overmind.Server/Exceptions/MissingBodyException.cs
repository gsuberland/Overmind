namespace Overmind.Server.Exceptions
{
    class MissingBodyException : OvermindException
    {
        public MissingBodyException() : base("No body was sent with the HTTP POST request.")
        {
            base.StatusCode = 422;
        }
    }
}
