using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures; 

[PublicAPI]
internal class TableListResponse {
    [JsonProperty("tables")]
    public List<string> Tables { get; private set; }
}
