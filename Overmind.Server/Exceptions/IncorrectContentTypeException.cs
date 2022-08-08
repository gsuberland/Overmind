namespace Overmind.Server.Exceptions
{
    class IncorrectContentTypeException : OvermindException
    {
        public IncorrectContentTypeException() : base("Incorrect content type. Must be application/json.")
        {
            base.StatusCode = 415;
        }
    }
}
