using System;
using JetBrains.Annotations;

namespace PullCanvasData2;

/// <summary>
/// Controls how requests are retried when CD2 responds with 429 (Too Many Requests) or a 5xx error.
/// </summary>
/// <remarks>
/// CD2 enforces per-endpoint, per-minute rate limits
/// (https://developerdocs.instructure.com/services/dap/limits-policies), responding with 429 if the limits are exceeded.
/// </remarks>
[PublicAPI]
public class RetryOptions {
    /// <summary>
    /// How many times a failed request is retried before giving up. 0 disables retrying.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Initial delay for exponential backoff, used when a <c>retry-after</c> is missing from the response.
    /// </summary>
    public TimeSpan BaseBackoffDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay for exponential backoff.
    /// </summary>
    public TimeSpan MaxBackoffDelay { get; set; } = TimeSpan.FromSeconds(60);
}
