using System.Net;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Messages;
using Overmind.Server.Config;

namespace Overmind.Server.Web
{
    class SecurityTokenDispatch : IWebDispatch
    {
        private const string ConfigServiceName = @"token";


        /// <summary>
        /// Dispatch a security token service request.
        /// </summary>
        /// <param name="serviceName">Service name. Must be "token".</param>
        /// <param name="context">HTTP context for the request.</param>
        public void Dispatch(string serviceName, HttpListenerContext context)
        {
            if (serviceName != ConfigServiceName)
                return;
            
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Handling dispatched request: {context.Request.RawUrl}");

            // create a new security token (this is for CSRF)
            (var token, var expiry) = SecurityTokenManager.Create();

            JsonSerializer.Serialize<SecurityTokenResponse>(
                context.Response.OutputStream,
                new SecurityTokenResponse(token, expiry),
                JsonSettings.SerializerOptions
            );
        }
    }
}
