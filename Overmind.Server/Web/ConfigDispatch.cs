using System.Net;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Server.Config;

namespace Overmind.Server.Web
{
    class ConfigDispatch : IWebDispatch
    {
        private const string ConfigServiceName = @"config";


        /// <summary>
        /// Dispatch a config service request.
        /// </summary>
        /// <param name="serviceName">Service name. Must be "config".</param>
        /// <param name="context">HTTP context for the request.</param>
        public void Dispatch(string serviceName, HttpListenerContext context)
        {
            if (serviceName != ConfigServiceName)
                return;
            
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Handling dispatched request: {context.Request.RawUrl}");

            JsonSerializer.Serialize<OvermindConfig>(
                context.Response.OutputStream,
                Program.Config,
                JsonSettings.SerializerOptions
            );
        }
    }
}
