using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures; 

[PublicAPI]
public class JobObject {
    [JsonProperty("id")]
    public string Id { get; private set; }    
}
