using System.Collections.ObjectModel;
using System.Reflection;

namespace Overmind.Messages
{
    public class ErrorResponse : OvermindResponse
    {
        public string ErrorMessage { get; protected set; }
        public string? ExceptionType { get; protected set; }
        public ReadOnlyDictionary<string, string?> ErrorProperties { get; protected set; }
        public string? StackTrace { get; protected set; }

        public ErrorResponse(Exception exception)
        {
            Success = false;
            ErrorMessage = exception.Message;
            ExceptionType = exception.GetType().Name;
            var errorProperties = new Dictionary<string, string?>();
            var exceptionProperties = exception.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var exceptionProperty in exceptionProperties)
            {
                var propertyName = exceptionProperty.Name;
                var propertyValue = exceptionProperty.GetValue(exception)?.ToString();
                errorProperties.Add(propertyName, propertyValue);
            }
            ErrorProperties = new ReadOnlyDictionary<string, string?>(errorProperties);
            StackTrace = exception.StackTrace;
        }
    }
}