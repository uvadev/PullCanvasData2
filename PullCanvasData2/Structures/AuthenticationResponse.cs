using System;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures {
    internal class AuthenticationResponse {
        [JsonProperty("access_token")]
        public string AccessToken { get; private set; }
        
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; private set; }
        
        [JsonProperty("scope")]
        public string Scope { get; private set; }
        
        [JsonProperty("token_type")]
        public string TokenType { get; private set; }

        public DateTime CreatedAt { get; }
        
        public DateTime ExpiresAt => CreatedAt.AddSeconds(ExpiresIn);

        [JsonConstructor]
        public AuthenticationResponse() {
            CreatedAt = DateTime.Now;
        }
    }
}