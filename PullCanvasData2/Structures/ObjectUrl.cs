using Newtonsoft.Json;

namespace PullCanvasData2.Structures; 

internal class ObjectUrl {
    [JsonProperty("url")]
    public string Url { get; private set; }
}
