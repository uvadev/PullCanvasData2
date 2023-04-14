using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PullCanvasData2.Structures {
    [PublicAPI]
    public class SnapshotJob : Job {
        [JsonProperty("at")]
        public DateTime? At { get; private set; }
    }
}
