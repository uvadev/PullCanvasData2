using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PullCanvasData2.Structures;

namespace PullCanvasData2; 

[PublicAPI]
public class CanvasData {
    private readonly HttpClient client;
        
    private readonly string clientId;
    private readonly string clientSecret;

    private AuthenticationResponse token;
        
    public CanvasData(string baseUrl, string clientId, string clientSecret) {
        client = new HttpClient();
        this.clientId = clientId;
        this.clientSecret = clientSecret;

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task Authenticate() {
        try {
            var args = new[] {
                ("grant_type", "client_credentials")
            };

            client.DefaultRequestHeaders.Remove("x-instauth");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                 "Basic", 
                 Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientId + ":" + clientSecret))
             );
            var response = await client.PostAsync("ids/auth/login", BuildHttpArguments(args));

            AssertSuccess(response);
            
            token = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
        } finally {
            client.DefaultRequestHeaders.Authorization = null;
            client.DefaultRequestHeaders.Add("x-instauth", token.AccessToken);
        }
    }

    public async Task<List<string>> GetTableList() {
        var response = await client.GetAsync("/dap/query/canvas/table");
        
        AssertSuccess(response);
        
        var data = JsonConvert.DeserializeObject<TableListResponse>(await response.Content.ReadAsStringAsync());
        return data.Tables;
    }

    public async Task<JObject> GetTableSchema(string tableName) {
        var response = await client.GetAsync($"/dap/query/canvas/table/{tableName}/schema");
        
        AssertSuccess(response);

        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }

    public async Task<SnapshotJob> PostSnapshotJob(string tableName, DataFormat format = DataFormat.JsonLines) {
        var response = await client.PostAsync(
            $"/dap/query/canvas/table/{tableName}/data", 
            BuildHttpJsonBody(JObject.FromObject(new { format = FormatToString(format) }))
        );

        AssertSuccess(response);
        
        return JsonConvert.DeserializeObject<SnapshotJob>(await response.Content.ReadAsStringAsync());
    }
    
    public async Task<IncrementalJob> PostIncrementalJob(string tableName, DateTime since, DataFormat format = DataFormat.JsonLines) {
        var response = await client.PostAsync(
            $"/dap/query/canvas/table/{tableName}/data", 
            BuildHttpJsonBody(JObject.FromObject(new { format = FormatToString(format), since }))
        );

        AssertSuccess(response);
        
        return JsonConvert.DeserializeObject<IncrementalJob>(await response.Content.ReadAsStringAsync());
    }

    public Task<T> GetJobStatus<T>(T job) where T: Job {
        return GetJobStatus<T>(job.Id);
    }
    
    public async Task<T> GetJobStatus<T>(string jobId) where T: Job {
        var response = await client.GetAsync($"/dap/job/{jobId}");
        
        AssertSuccess(response);
        
        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    public async Task<T> AwaitJobCompletion<T>(string jobId, int pollingIntervalMs = 1000) where T: Job {
        return await AwaitJobCompletion(await GetJobStatus<T>(jobId), pollingIntervalMs);
    }

    public async Task<T> AwaitJobCompletion<T>(T job, int pollingIntervalMs = 1000) where T: Job {
        while (job.Status != JobStatus.Complete && job.Status != JobStatus.Failed) {
            await Task.Delay(pollingIntervalMs);
            job = await GetJobStatus(job);
        }

        return job;
    }

    public async Task<Dictionary<string, string>> GetJobUrls(Job j) {
        if (j.Objects == null) {
            throw new CanvasDataException("GetObjectUrls() called on incomplete or failed job.");
        }
        
        var response = await client.PostAsync(
            "/dap/object/url", 
            BuildHttpJsonBody(JArray.FromObject(j.Objects))
        );
        
        var data = JsonConvert.DeserializeObject<ObjectUrlsResponse>(await response.Content.ReadAsStringAsync());
        
        return data.Urls.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.Url))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public Task<Stream> StreamUrl(string url) {
        return client.GetStreamAsync(url);
    }

    private static void AssertSuccess(HttpResponseMessage response) {
        if (!response.IsSuccessStatusCode) {
            throw new CanvasDataException(response.Content.ReadAsStringAsync().Result);
        }
    }
        
    private static HttpContent BuildHttpArguments([NotNull] IEnumerable<(string, string)> args) {

        var pairs = args.Where(a => a.Item2 != null)
                        .Select(a => new KeyValuePair<string, string>(a.Item1, a.Item2));

        var content = new FormUrlEncodedContent(pairs);
            
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        return content;
    }
    
    private static HttpContent BuildHttpJsonBody(JToken json) {
        return new StringContent(json.ToString(), Encoding.Default, "application/json");
    }

    private static string FormatToString(DataFormat dataFormat) {
        return dataFormat switch {
            DataFormat.Tsv => "tsv",
            DataFormat.Csv => "csv",
            DataFormat.JsonLines => "jsonl",
            DataFormat.Parquet => "parquet",
            _ => throw new ArgumentOutOfRangeException(nameof(dataFormat), dataFormat, null)
        };
    }
}