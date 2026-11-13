using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PullCanvasData2;

internal class RetryHandler : DelegatingHandler {
    private const int TooManyRequests = 429;

    private readonly RetryOptions options;

    internal RetryHandler(RetryOptions options) : base(new HttpClientHandler()) {
        this.options = options ?? new RetryOptions();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken) {
        for (var attempt = 1;; attempt++) {
            HttpResponseMessage response = null;
            try {
                response = await base.SendAsync(request, cancellationToken);
            } catch (HttpRequestException) when (attempt <= options.MaxRetries) {
                // No response at all; retry with backoff
            }

            TimeSpan wait;
            if (response == null) {
                wait = Backoff(attempt);  // HttpRequestException case
                await Console.Error.WriteLineAsync($"Connection failed. Retrying in {wait.TotalSeconds:0.#} seconds.");
            } else {
                var status = (int) response.StatusCode;
                if (attempt > options.MaxRetries || status != TooManyRequests && status < 500) {
                    return response;
                }

                wait = await ServerRequestedDelay(response) ?? Backoff(attempt);
                response.Dispose();

                var reason = status == TooManyRequests ? "Rate limited"
                                                       : "Server error"; 
                await Console.Error.WriteLineAsync($"{reason}. Retrying in {wait.TotalSeconds:0.#} seconds.");
            }

            await Task.Delay(wait, cancellationToken);
        }
    }

    private TimeSpan Backoff(int attempt) {
        var ms = options.BaseBackoffDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        return TimeSpan.FromMilliseconds(Math.Min(ms, options.MaxBackoffDelay.TotalMilliseconds));
    }

    private static async Task<TimeSpan?> ServerRequestedDelay(HttpResponseMessage response) {
        var header = response.Headers.RetryAfter;
        
        if (header?.Delta != null) {
            return header.Delta.Value;
        }
        
        if (header?.Date != null) {
            var delta = header.Date.Value - DateTimeOffset.UtcNow;
            return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
        }

        if (response.StatusCode != (HttpStatusCode) TooManyRequests || response.Content == null) {
            return null;
        }
        
        // `retry-after` seems to occur in the response body as well: {"error":"Too Many Requests","retry-after":1}
        try {
            var body = JObject.Parse(await response.Content.ReadAsStringAsync());
            var seconds = body["retry-after"];
            if (seconds is { Type: JTokenType.Integer or JTokenType.Float }) {
                return TimeSpan.FromSeconds(Math.Max(0, seconds.Value<double>()));
            }
        } catch (Exception) {
            // Unexpected shape, so just rely on backoff.
        }
        
        return null;
    }
}
