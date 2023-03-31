using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
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
        var args = new[] {
            ("grant_type", "client_credentials")
        };

        client.DefaultRequestHeaders.Remove("x-instauth");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientId + ":" + clientSecret)));
        var response = await client.PostAsync("ids/auth/login", BuildHttpArguments(args));
        client.DefaultRequestHeaders.Authorization = null;

        if (!response.IsSuccessStatusCode) {
            throw new Exception("Auth failed: " + await response.Content.ReadAsStringAsync());
        }
            
        token = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Add("x-instauth", token.AccessToken);
    }
        
    private static HttpContent BuildHttpArguments([NotNull] IEnumerable<(string, string)> args) {

        var pairs = args.Where(a => a.Item2 != null)
                        .Select(a => new KeyValuePair<string, string>(a.Item1, a.Item2));

        var content = new FormUrlEncodedContent(pairs);
            
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        return content;
    }
}