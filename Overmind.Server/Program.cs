using log4net;
using Overmind.Server.Config;
using Overmind.Server.Web;

namespace Overmind.Server
{
    static class Program
    {
        public static OvermindConfig Config { get; private set; }
        public static WebServer[] Servers { get; private set; }
        private static ManualResetEvent _exitEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
            _log.Info("Starting up...");

            var config = OvermindConfig.Load("overmind.json");
            if (config == null)
            {
                Console.Error.WriteLine("Error while loading configuration. Exiting.");
                return;
            }
            Program.Config = config;

            Servers = new WebServer[config.Servers.Length];
            int serverIndex = 0;
            foreach (var serverConfig in config.Servers)
            {
                Servers[serverIndex] = new WebServer(serverConfig);
                Console.WriteLine($"Starting server on port {serverConfig.Port}");
                Servers[serverIndex].Start();
            }

            Console.CancelKeyPress += ControlCPressed;

            _exitEvent.WaitOne();
        }

        private static void ControlCPressed(object sender, ConsoleCancelEventArgs args)
        {
            _exitEvent.Set();
        }
    }
}