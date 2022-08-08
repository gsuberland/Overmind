using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using log4net;
using Overmind.Server.Config;
using Overmind.Server.Exceptions;
using Overmind.Messages;

namespace Overmind.Server.Web
{
    class WebServer
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private List<IWebDispatch> _dispatchers;
        private readonly HttpListener _listener;
        private readonly WebServerConfig _config;

        public WebServer(WebServerConfig config)
        {
            _config = config;
            _listener = new HttpListener();
            _dispatchers = new List<IWebDispatch>();

            string prefixAddress = $"http{((config.Secure ?? false) ? "s" : "")}://{config.Host ?? "localhost"}:{config.Port}/";
            _log.Info($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Initialising webserver with listener prefix {prefixAddress}");
            _listener.Prefixes.Add(prefixAddress);

            // if a user & pass are specified, enable Basic Authentication
            if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
            {
                _listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
            }

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
                // verify that the correct encoding is being used
                if (context.Request.ContentEncoding != Encoding.UTF8)
                {
                    throw new InvalidRequestEncodingException();
                }

                // verify that the request is either a GET or a POST (dispatcher can additionally validate this)
                if (context.Request.HttpMethod != "GET" && context.Request.HttpMethod != "POST")
                {
                    throw new IncorrectVerbException();
                }

                // if a username and password have been configured, requrire auth
                if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
                {
                    // get the identity from the request
                    var identity = context?.User?.Identity as HttpListenerBasicIdentity;
                    if (identity == null)
                        throw new AccessDeniedException();

                    // constant-time compare of user & password
                    byte[] correctUsername = Encoding.UTF8.GetBytes(_config.Username);
                    byte[] correctPassword = Encoding.UTF8.GetBytes(_config.Password);
                    
                    byte[] testUsername = Encoding.UTF8.GetBytes(identity.Name);
                    byte[] testPassword = Encoding.UTF8.GetBytes(identity.Password);

                    int compare = 0;
                    int usernameCompareLength = Math.Min(correctUsername.Length, testUsername.Length);
                    for (int i = 0; i < usernameCompareLength; i++)
                    {
                        compare |= correctUsername[i] ^ testUsername[i];
                    }
                    compare |= correctUsername.Length ^ testUsername.Length;

                    int passwordCompareLength = Math.Min(correctPassword.Length, testPassword.Length);
                    for (int i = 0; i < passwordCompareLength; i++)
                    {
                        compare |= correctPassword[i] ^ testPassword[i];
                    }
                    compare |= correctPassword.Length ^ testPassword.Length;

                    if (compare != 0)
                    {
                        throw new AccessDeniedException();
                    }
                }

                // if the request is not to the token service, a security token MUST be supplied
                if (serviceName != "token" && !SecurityTokenManager.Validate(context.Request.Headers.Get("Security-Token")))
                {
                    throw new InvalidSecurityTokenException();
                }

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
