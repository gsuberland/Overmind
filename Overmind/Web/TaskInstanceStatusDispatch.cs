using System.Net;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Config;
using Overmind.Exceptions;
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
                throw new InvalidRequestParameterException("id");
            }

            // try to get the task instance from the provided guid
            TaskInstance? taskInstance = TaskManager.GetInstance(taskInstanceGuid);
            if (taskInstance == null)
            {
                // no task found by that id
                throw new TaskInstanceNotFoundException(taskInstanceGuid);
            }

            // task found! return it :)
            context.Response.AddHeader("Location", context.Request.Url.GetLeftPart(UriPartial.Authority) + "/status/" + taskInstanceGuid.ToString());
            JsonSerializer.Serialize(
                context.Response.OutputStream,
                taskInstance.ToResponse(),
                JsonSettings.SerializerOptions
            );
        }
    }
}
