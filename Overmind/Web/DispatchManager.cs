using System.Reflection;
using log4net;
using Overmind.Config;

namespace Overmind.Web
{
    static class DispatchManager
    {
        public static void Init(WebServer server)
        {
            // find all the dispatcher classes in the assembly, using reflection
            // only check the currently executing assembly; we don't want to look through any other assemblies as that might lead to security issues
            var dispatcherTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsInterface && t.IsClass && !t.IsAbstract && !t.IsNested && !t.IsGenericType && typeof(IWebDispatch).IsAssignableFrom(t));

            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            
            // go through each type, create an instance of it, and register it to the server
            foreach (var dispatcherType in dispatcherTypes)
            {
                // this shouldn't happen, since a null in the dispatcher types makes no sense, but catch just in case
                if (dispatcherType == null)
                {
                    _log.Warn($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Null type in dispatcher list.");
                    continue;
                }
                
                // make a new instance of the dispatcher type
                _log.Debug($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Creating instance of dispatch class {dispatcherType.FullName}");
                IWebDispatch? dispatcher = (IWebDispatch?)Activator.CreateInstance(dispatcherType);
                if (dispatcher == null)
                {
                    _log.Warn($"[{System.Reflection.MethodBase.GetCurrentMethod()?.Name}] Instantiating {dispatcherType.Name} resulted in null result");
                    continue;
                }

                // register this dispatcher type
                server.RegisterDispatcher(dispatcher);
            }
        }
    }
}
