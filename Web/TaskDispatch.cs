using System.Net;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Config;
using Overmind.Tasks;

namespace Overmind.Web
{
    class TaskDispatch : IWebDispatch
    {
        private const string ConfigServiceName = @"task";

        /// <summary>
        /// Dispatch a task service request.
        /// </summary>
        /// <param name="serviceName">Service name. Must be "task".</param>
        /// <param name="context">HTTP context for the request.</param>
        public void Dispatch(string serviceName, HttpListenerContext context)
        {
            if (serviceName != ConfigServiceName)
                return;
            
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Handling dispatched request: {context.Request.RawUrl}");

            var args = context.Request.Url.Query
                .TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(p => p.Split('=').First(), p => p.Split('=').Skip(1).FirstOrDefault() ?? "");
            string taskName = (context.Request.Url.Segments.Skip(2).FirstOrDefault() ?? "").TrimEnd('/');

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            TaskInstance task;
            try
            {
                task = TaskManager.Start(taskName, args);
                JsonSerializer.Serialize<TaskInstance>(context.Response.OutputStream, task);
            }
            catch (InvalidTaskParameterException pex)
            {
                JsonSerializer.Serialize(context.Response.OutputStream, new { Error = true, ExceptionType = pex.GetType().Name, Exception = pex.Message, StackTrace = pex.StackTrace });
            }
            catch (Exception ex)
            {
                // todo: return generic error if debug mode disabled
                JsonSerializer.Serialize(context.Response.OutputStream, new { Error = true, ExceptionType = ex.GetType().Name, Exception = ex.Message, StackTrace = ex.StackTrace });
            }
            context.Response.OutputStream.Flush();
            context.Response.Close();
        }
    }
}
