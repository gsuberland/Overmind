namespace Overmind.Server.Exceptions
{
    class AccessDeniedException : OvermindException
    {
        public AccessDeniedException() : base("Access denied.")
        {
            base.StatusCode = 401;
        }
    }
}
