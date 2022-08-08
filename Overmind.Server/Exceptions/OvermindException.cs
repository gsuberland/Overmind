namespace Overmind.Server.Exceptions
{
    abstract class OvermindException : Exception
    {
        protected OvermindException(string message) : base(message)
        {

        }

        /// <summary>
        /// The HTTP status code to set as a result of this exception.
        /// </summary>
        public int? StatusCode { get; protected set; }
    }
}