using System.Net;
using System.Net.Http;
using System.Text;
using log4net;
using Overmind.Config;

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
            // broadcast this request to all the registered dispatchers
            foreach (var dispatcher in _dispatchers)
            {
                dispatcher.Dispatch(serviceName, context);
            }
        }
    }
}
