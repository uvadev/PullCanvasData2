using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures; 

[PublicAPI]
public class ProcessingError {
    
    [JsonProperty("type")]
    public string Type { get; private set; }
    
    [JsonProperty("uuid")]
    public string Uuid { get; private set; }
    
    [JsonProperty("message")]
    public string Message { get; private set; }
}
