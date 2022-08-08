namespace Overmind.Server.Exceptions
{
    class InvalidSecurityTokenException : OvermindException
    {
        public InvalidSecurityTokenException() : base("No valid security token was supplied.")
        {
            base.StatusCode = 403;
        }
    }
}
