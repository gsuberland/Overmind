using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Config;
using Overmind.Exceptions;
using Overmind.Messages;

namespace Overmind.Web
{
    class WebServer
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private List<IWebDispatch> _dispatchers;
        private readonly HttpListener _listener;

        public WebServer(WebServerConfig config)
        {
            _listener = new HttpListener();
            _dispatchers = new List<IWebDispatch>();

            string prefixAddress = $"http{((config.Secure ?? false) ? "s" : "")}://{config.Host ?? "localhost"}:{config.Port}/";
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Initialising webserver with listener prefix {prefixAddress}");
            _listener.Prefixes.Add(prefixAddress);

            // register dispatcher types to this server (these handle the requests)
            DispatchManager.Init(this);
        }

        public void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(DispatchRequest, null);
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public void RegisterDispatcher(IWebDispatch dispatcher)
        {
            _dispatchers.Add(dispatcher);
        }

        private void DispatchRequest(IAsyncResult result)
        {
            // new connection came in; accept it and get the context
            var context = _listener.EndGetContext(result);
            // begin accepting the next connection
            _listener.BeginGetContext(DispatchRequest, null);

            // results will always be UTF-8 encoded JSON
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;

            // service name is the first part of the URL path
            string serviceName = (context.Request.Url?.Segments?.FirstOrDefault(s => s != "/") ?? "/").TrimEnd('/').ToLower();

            try
            {
                // broadcast this request to all the registered dispatchers
                foreach (var dispatcher in _dispatchers)
                {
                    dispatcher.Dispatch(serviceName, context);
                }
            }
            catch (OvermindException oex)
            {
                // we raised an exception somewhere; return an error.

                if (oex.StatusCode != null)
                {
                    context.Response.StatusCode = oex.StatusCode.Value;
                }
                else
                {
                    context.Response.StatusCode = 500;
                }

                JsonSerializer.Serialize(
                    context.Response.OutputStream,
                    new ErrorResponse(oex),
                    JsonSettings.SerializerOptions);
            }
            catch (Exception ex)
            {
                // unexpected exception occurred
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";

                // todo: return generic error if debug mode disabled
                JsonSerializer.Serialize(
                    context.Response.OutputStream, 
                    new ErrorResponse(ex),
                    JsonSettings.SerializerOptions);
            }
            finally
            {
                context.Response.OutputStream.Flush();
                context.Response.Close();
            }
        }
    }
}
