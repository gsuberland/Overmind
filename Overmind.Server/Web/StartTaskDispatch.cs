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

            // pull out the arguments from the URL
            // todo: switch to POST and JSON input here!
            var args = context.Request.Url.Query
                .TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(p => p.Split('=').First(), p => p.Split('=').Skip(1).FirstOrDefault() ?? "");
            
            // pull the task name from the query (i.e. /start/taskName -> taskName)
            string taskName = (context.Request.Url.Segments.Skip(2).FirstOrDefault() ?? "").TrimEnd('/');

            // start the task!
            TaskInstance task;
            task = TaskManager.Start(taskName, args);

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
