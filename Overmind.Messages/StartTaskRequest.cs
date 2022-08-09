using System.Collections.ObjectModel;

namespace Overmind.Messages
{
    public class StartTaskRequest
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public Uri? CallbackUrl { get; set; }
    }
}
