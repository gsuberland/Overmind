using System.Net;
using System.Net.Http;
using log4net;
using Overmind.Config;

namespace Overmind.Web
{
    interface IWebDispatch
    {
        public void Dispatch(string serviceName, HttpListenerContext context);
    }
}