using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MEA2100_Recording_and_Stimulation
{
    /// <summary>
    /// Provides HTTP request functionality with automatic retry and rate-limit handling.
    /// Handles HTTP 429 (Too Many Requests) responses by respecting Retry-After headers
    /// and applying exponential backoff, while distinguishing transient rate limits from
    /// permanent quota exhaustion.
    /// </summary>
    public class HttpApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly int _maxRetries;
        private readonly TimeSpan _initialDelay;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of <see cref="HttpApiClient"/> with an internally managed <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts on HTTP 429. Defaults to 3.</param>
        /// <param name="initialDelay">Initial backoff delay before the first retry. Defaults to 1 second.</param>
        public HttpApiClient(int maxRetries = 3, TimeSpan? initialDelay = null)
            : this(new HttpClient(), ownsHttpClient: true, maxRetries, initialDelay)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HttpApiClient"/> with an externally supplied <see cref="HttpClient"/>.
        /// The supplied <paramref name="httpClient"/> is <em>not</em> disposed when this instance is disposed.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use for requests.</param>
        /// <param name="maxRetries">Maximum number of retry attempts on HTTP 429. Defaults to 3.</param>
        /// <param name="initialDelay">Initial backoff delay before the first retry. Defaults to 1 second.</param>
        public HttpApiClient(HttpClient httpClient, int maxRetries = 3, TimeSpan? initialDelay = null)
            : this(httpClient, ownsHttpClient: false, maxRetries, initialDelay)
        {
        }

        private HttpApiClient(HttpClient httpClient, bool ownsHttpClient, int maxRetries, TimeSpan? initialDelay)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ownsHttpClient = ownsHttpClient;
            _maxRetries = maxRetries;
            _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Sends an HTTP request and retries on HTTP 429 responses using exponential backoff.
        /// </summary>
        /// <param name="requestFactory">
        /// A factory function that creates a new <see cref="HttpRequestMessage"/> for each attempt.
        /// A new message must be created per attempt because <see cref="HttpRequestMessage"/> cannot be resent.
        /// </param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The successful <see cref="HttpResponseMessage"/>.</returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the request fails with a non-retryable error, the quota is exhausted,
        /// or all retry attempts are consumed.
        /// </exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        public async Task<HttpResponseMessage> SendRequestWithRetryAsync(
            Func<HttpRequestMessage> requestFactory,
            CancellationToken cancellationToken = default)
        {
            if (requestFactory == null)
                throw new ArgumentNullException(nameof(requestFactory));

            int attempt = 0;
            TimeSpan delay = _initialDelay;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                HttpResponseMessage response = await _httpClient
                    .SendAsync(requestFactory(), cancellationToken)
                    .ConfigureAwait(false);

                if (response.StatusCode != (HttpStatusCode)429)
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        response.Dispose();
                        throw;
                    }
                    return response;
                }

                // HTTP 429: check whether this is a permanent quota error or a transient limit.
                string errorBody = null;
                using (response)
                {
                    if (response.Content != null)
                    {
                        errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    if (IsQuotaExhausted(errorBody))
                    {
                        throw new HttpRequestException(
                            "API quota exhausted (HTTP 429 insufficient_quota). " +
                            "Please check your plan and billing details. " +
                            "Original response: " + errorBody);
                    }

                    attempt++;
                    if (attempt > _maxRetries)
                    {
                        throw new HttpRequestException(
                            $"Request failed with HTTP 429 after {_maxRetries} retries. " +
                            "Original response: " + errorBody);
                    }

                    // Honor the Retry-After header when present; otherwise use exponential backoff.
                    TimeSpan waitTime = GetRetryAfterDelay(response) ?? delay;
                    await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
                }

                // Double the backoff for the next attempt (capped at 60 seconds).
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
            }
        }

        /// <summary>
        /// Determines whether an HTTP 429 response body indicates a permanent quota exhaustion
        /// rather than a transient rate limit.
        /// </summary>
        private static bool IsQuotaExhausted(string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody))
                return false;

            // OpenAI and several other APIs signal quota exhaustion via these strings.
            return responseBody.IndexOf("insufficient_quota", StringComparison.OrdinalIgnoreCase) >= 0
                || responseBody.IndexOf("quota_exceeded", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Reads the <c>Retry-After</c> header from the response if present.
        /// Supports both delta-seconds and HTTP-date formats.
        /// </summary>
        private static TimeSpan? GetRetryAfterDelay(HttpResponseMessage response)
        {
            if (response.Headers.RetryAfter == null)
                return null;

            if (response.Headers.RetryAfter.Delta.HasValue)
                return response.Headers.RetryAfter.Delta.Value;

            if (response.Headers.RetryAfter.Date.HasValue)
            {
                TimeSpan remaining = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (remaining > TimeSpan.Zero)
                    return remaining;
            }

            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_ownsHttpClient)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
