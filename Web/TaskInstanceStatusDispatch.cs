using System.Net;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Config;
using Overmind.Tasks;

namespace Overmind.Web
{
    class TaskInstanceStatusDispatch : IWebDispatch
    {
        private const string ConfigServiceName = @"status";

        /// <summary>
        /// Dispatch a task instance status service request.
        /// </summary>
        /// <param name="serviceName">Service name. Must be "status".</param>
        /// <param name="context">HTTP context for the request.</param>
        public void Dispatch(string serviceName, HttpListenerContext context)
        {
            if (serviceName != ConfigServiceName)
                return;
            
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Handling dispatched request: {context.Request.RawUrl}");

            string taskInstanceIdStr = (context.Request.Url.Segments.Skip(2).FirstOrDefault() ?? "").TrimEnd('/');
            Guid taskInstanceGuid;
            if (!Guid.TryParse(taskInstanceIdStr, out taskInstanceGuid))
            {
                JsonSerializer.Serialize(
                    context.Response.OutputStream,
                    new { 
                        Error = true, 
                        ExceptionType = typeof(InvalidDataException).Name, 
                        Exception = "Invalid task instance GUID."
                    },
                    JsonSettings.SerializerOptions
                );
                return;
            }

            // try to get the task instance from the provided guid
            TaskInstance? taskInstance = TaskManager.GetInstance(taskInstanceGuid);
            if (taskInstance == null)
            {
                // no task found by that id, return 404 and null response
                context.Response.StatusCode = 404;
                JsonSerializer.Serialize(
                    context.Response.OutputStream,
                    (object?)null,
                    JsonSettings.SerializerOptions
                );
                return;
            }

            // task found! return it :)
            JsonSerializer.Serialize(
                context.Response.OutputStream,
                taskInstance,
                JsonSettings.SerializerOptions
            );

            context.Response.OutputStream.Flush();
            context.Response.Close();
        }
    }
}
