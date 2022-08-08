using System.Collections.ObjectModel;

namespace Overmind.Messages
{
    public class SecurityTokenResponse : OvermindResponse
    {
        public string Token { get; protected set; }
        public DateTime Expiry { get; protected set; }

        public SecurityTokenResponse(string token, DateTime expiry)
        {
            this.Success = true;
            this.Token = token;
            this.Expiry = expiry;
        }
    }
}