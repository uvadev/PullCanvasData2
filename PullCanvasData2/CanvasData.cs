using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PullCanvasData2.Structures;
using static PullCanvasData2.Util;

namespace PullCanvasData2 {
    [PublicAPI]
    public class CanvasData {
        private readonly HttpClient client;
        private readonly AuthMonitor authMonitor;
        
        public CanvasData(string baseUrl, string clientId, string clientSecret) {
            client = new HttpClient();
            authMonitor = new AuthMonitor(baseUrl, clientId, clientSecret);

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task Authenticate() {
            await authMonitor.Authenticate();
        }

        private async Task<HttpResponseMessage> GetWithAuthAsync(string url) {
            return await client.GetAsyncWithHeaders(url, new[] {
                ("x-instauth", await authMonitor.GetAccessToken())
            });
        }
        
        private async Task<HttpResponseMessage> PostWithAuthAsync(string url, HttpContent content) {
            return await client.PostAsyncWithHeaders(url, content, new[] {
                ("x-instauth", await authMonitor.GetAccessToken())
            });
        }

        public async Task<List<string>> GetTableList() {
            var response = await GetWithAuthAsync("/dap/query/canvas/table");
        
            AssertSuccess(response);
        
            var data = JsonConvert.DeserializeObject<TableListResponse>(await response.Content.ReadAsStringAsync());
            return data.Tables;
        }

        public async Task<JObject> GetTableSchema(string tableName) {
            var response = await GetWithAuthAsync($"/dap/query/canvas/table/{tableName}/schema");
        
            AssertSuccess(response);

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<SnapshotJob> PostSnapshotJob(string tableName, DataFormat format = DataFormat.JsonLines) {
            var response = await PostWithAuthAsync(
                                                   $"/dap/query/canvas/table/{tableName}/data", 
                                                   BuildHttpJsonBody(JObject.FromObject(new { format = FormatToString(format) }))
                                                   );

            AssertSuccess(response);
        
            return JsonConvert.DeserializeObject<SnapshotJob>(await response.Content.ReadAsStringAsync());
        }
    
        public async Task<IncrementalJob> PostIncrementalJob(string tableName, DateTime since, DataFormat format = DataFormat.JsonLines) {
            var response = await PostWithAuthAsync(
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
            var response = await GetWithAuthAsync($"/dap/job/{jobId}");
        
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
        
            var response = await PostWithAuthAsync(
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
}