using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Overmind.Messages;

namespace Overmind.Server.Tasks
{
    static class CallbackManager
    {
        private static readonly ConcurrentQueue<Tuple<Uri, TaskInstanceResponse>> _callbackQueue = new ConcurrentQueue<Tuple<Uri, TaskInstanceResponse>>();

        public static void ScheduleCallback(TaskInstance task)
        {
            if (task.CallbackUrl == null)
            {
                // callback URL not set, so don't do a callback
                return;
            }
            
            if ((Program.Config.CallbackDomains?.Length ?? 0) == 0)
            {
                // no callback domains have been whitelisted
                return;
            }

            bool callbackMatchesWhitelistedDomain = Program.Config.CallbackDomains?.Any(cd => cd.Equals(task.CallbackUrl.Host, StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!callbackMatchesWhitelistedDomain)
            {
                // callback provided by user does not match a whitelisted domain, so don't send anything to it.
                return;
            }

            // add the callback sender
            _callbackQueue.Enqueue(
                new Tuple<Uri, TaskInstanceResponse>(
                    task.CallbackUrl,
                    task.ToResponse()
                )
            );

            // async sender
            Task.Run(CallbackSender);
        }

        private static void CallbackSender()
        {
            Tuple<Uri, TaskInstanceResponse>? callback;
            // keep getting callbacks from the queue until there are none left
            while (_callbackQueue.TryDequeue(out callback))
            {
                if (callback == null)
                    continue;
                
                var uri = callback.Item1;
                var response = callback.Item2;

                try
                {
                    using (var hc = new HttpClient())
                    using (var msg = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        msg.Content = new StringContent(
                            JsonSerializer.Serialize<TaskInstanceResponse>(response),
                            Encoding.UTF8,
                            "application/json"
                        );
                        using (var httpResponse = hc.Send(msg))
                        {
                            // do nothing with the response for now, I guess
                        }
                    }
                }
                catch 
                {
                    // ... at some point I should probably log this
                }
            }
        }
    }
}