namespace Overmind.Server.Exceptions
{
    class InvalidRequestEncodingException : OvermindException
    {
        public InvalidRequestEncodingException() : base("Invalid character encoding for request; must use UTF-8.")
        {
            base.StatusCode = 415;
        }
    }
}
