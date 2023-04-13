using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures; 

[PublicAPI]
public class IncrementalJob : Job {
    [JsonProperty("since")]
    public DateTime? Since { get; private set; }
    
    [JsonProperty("until")]
    public DateTime? Until { get; private set; }
}
