using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace PullCanvasData2.Structures; 

[PublicAPI]
public class Job {
    [JsonProperty("id")]
    public string Id { get; private set; }
    
    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(SnakeCaseNamingStrategy))]
    public JobStatus Status { get; private set; }
    
    [JsonProperty("expires_at")]
    public DateTime? ExpiresAt { get; private set; }
    
    [JsonProperty("schema_version")]
    public int? SchemaVersion { get; private set; }

    [JsonProperty("error")]
    [CanBeNull]
    public ProcessingError Error { get; private set; }
    
    [JsonProperty("objects")]
    [CanBeNull]
    public List<JobObject> Objects { get; private set; }
}
