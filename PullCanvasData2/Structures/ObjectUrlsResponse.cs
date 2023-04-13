using System.Collections.Generic;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures; 

internal class ObjectUrlsResponse {
    [JsonProperty("urls")]
    public Dictionary<string, ObjectUrl> Urls { get; private set; }
}
