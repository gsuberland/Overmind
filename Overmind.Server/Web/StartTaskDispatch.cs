using System.Net;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Server.Config;
using Overmind.Server.Tasks;
using Overmind.Server.Exceptions;
using Overmind.Messages;

namespace Overmind.Server.Web
{
    class StartTaskDispatch : IWebDispatch
    {
        private const string ConfigServiceName = @"start";

        /// <summary>
        /// Dispatch a start task service request.
        /// </summary>
        /// <param name="serviceName">Service name. Must be "start".</param>
        /// <param name="context">HTTP context for the request.</param>
        public void Dispatch(string serviceName, HttpListenerContext context)
        {
            if (serviceName != ConfigServiceName)
                return;
            
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Handling dispatched request: {context.Request.RawUrl}");

            if (context.Request.HttpMethod != "POST")
            {
                throw new IncorrectVerbException();
            }

            if (context.Request.ContentType != "application/json")
            {
                throw new IncorrectContentTypeException();
            }
            if (!context.Request.HasEntityBody)
            {
                throw new MissingBodyException();
            }

            var startRequest = JsonSerializer.Deserialize<StartTaskRequest>(
                context.Request.InputStream,
                JsonSettings.SerializerOptions);
            
            if (startRequest == null)
            {
                throw new InvalidRequestParameterException("");
            }
            if (string.IsNullOrEmpty(startRequest.Name))
            {
                throw new InvalidRequestParameterException("Name");
            }
            if (startRequest.Parameters == null)
            {
                throw new InvalidRequestParameterException("Parameters");
            }

            // try to start the task!
            TaskInstance task;
            task = TaskManager.Start(startRequest.Name, startRequest.Parameters, startRequest.CallbackUrl);

            // we got a task instance, so show it
            context.Response.AddHeader("Location", context.Request.Url.GetLeftPart(UriPartial.Authority) + "/status/" + task.Id.ToString());
            JsonSerializer.Serialize<TaskInstanceResponse>(
                context.Response.OutputStream,
                task.ToResponse(),
                JsonSettings.SerializerOptions
            );
        }
    }
}
