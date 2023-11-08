using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace PullCanvasData2; 

internal static class Util {

    internal static async Task<HttpResponseMessage> GetAsyncWithHeaders(this HttpClient client, string url, IEnumerable<(string, string)> headers) {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        
        foreach (var (key, val) in headers) {
            requestMessage.Headers.Add(key, val);
        }

        return await client.SendAsync(requestMessage);
    }
    
    internal static async Task<HttpResponseMessage> PostAsyncWithHeaders(this HttpClient client, string url, HttpContent content, IEnumerable<(string, string)> headers) {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Content = content;

        foreach (var (key, val) in headers) {
            requestMessage.Headers.Add(key, val);
        }

        return await client.SendAsync(requestMessage);
    }
    
    internal static HttpContent BuildHttpArguments([NotNull] IEnumerable<(string, string)> args) {
        var pairs = args.Where(a => a.Item2 != null)
                        .Select(a => new KeyValuePair<string, string>(a.Item1, a.Item2));

        var content = new FormUrlEncodedContent(pairs);
            
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        return content;
    }
    
    internal static HttpContent BuildHttpJsonBody(JToken json) {
        return new StringContent(json.ToString(), Encoding.Default, "application/json");
    }
    
    internal static void AssertSuccess(HttpResponseMessage response) {
        if (!response.IsSuccessStatusCode) {
            throw new CanvasDataException(response.Content.ReadAsStringAsync().Result);
        }
    }
        
    internal static void AssertSuccess(HttpResponseMessage response, Func<string, Exception> exceptionSupplier) {
        if (!response.IsSuccessStatusCode) {
            throw exceptionSupplier.Invoke(response.Content.ReadAsStringAsync().Result);
        }
    }
}
