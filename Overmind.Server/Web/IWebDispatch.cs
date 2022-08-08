using System.Net;
using System.Net.Http;
using log4net;
using Overmind.Server.Config;

namespace Overmind.Server.Web
{
    interface IWebDispatch
    {
        public void Dispatch(string serviceName, HttpListenerContext context);
    }
}