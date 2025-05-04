using System;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PSTeable.Utils
{
    /// <summary>
    /// Extensions for HttpClient to handle API requests
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sends a request to the API and handles common error cases
        /// </summary>
        /// <param name="client">The HttpClient instance</param>
        /// <param name="request">The HttpRequestMessage to send</param>
        /// <param name="cmdlet">The cmdlet that is making the request</param>
        /// <param name="respectRateLimit">Whether to respect rate limits</param>
        /// <param name="rateLimitDelay">The delay to use when rate limited</param>
        /// <returns>The HttpResponseMessage</returns>
        public static HttpResponseMessage SendWithErrorHandling(
            this HttpClient client,
            HttpRequestMessage request,
            PSCmdlet cmdlet,
            bool respectRateLimit = false,
            TimeSpan? rateLimitDelay = null)
        {
            Logger.Verbose(cmdlet, $"Sending {request.Method} request to {request.RequestUri}");

            HttpResponseMessage response = null;

            try
            {
                response = client.SendAsync(request).GetAwaiter().GetResult();

                // Handle rate limiting
                if ((int)response.StatusCode == 429 && respectRateLimit)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta;
                    var delay = retryAfter ?? rateLimitDelay ?? TimeSpan.FromSeconds(5);

                    Logger.Verbose(cmdlet, $"Rate limited. Retrying after {delay.TotalSeconds} seconds.");

                    Thread.Sleep(delay);

                    // Retry the request
                    response.Dispose();
                    var retryRequest = new HttpRequestMessage(request.Method, new Uri(request.RequestUri));
                    foreach (var header in request.Headers)
                    {
                        retryRequest.Headers.Add(header.Key, header.Value);
                    }

                    if (request.Content != null)
                    {
                        retryRequest.Content = request.Content;
                    }

                    response = client.SendAsync(retryRequest).GetAwaiter().GetResult();
                }

                // Handle other error responses
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = $"{(int)response.StatusCode} {response.ReasonPhrase}";

                    try
                    {
                        var errorContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(errorContent))
                        {
                            errorMessage += $": {errorContent}";
                        }
                    }
                    catch
                    {
                        // Ignore errors reading the error content
                    }

                    Logger.Error(cmdlet, errorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(cmdlet, $"Error sending request: {ex.Message}");
            }

            return response;
        }

        /// <summary>
        /// Sends a request to the API and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="client">The HttpClient instance</param>
        /// <param name="request">The HttpRequestMessage to send</param>
        /// <param name="cmdlet">The cmdlet that is making the request</param>
        /// <param name="respectRateLimit">Whether to respect rate limits</param>
        /// <param name="rateLimitDelay">The delay to use when rate limited</param>
        /// <returns>The deserialized response</returns>
        public static T SendAndDeserialize<T>(
            this HttpClient client,
            HttpRequestMessage request,
            PSCmdlet cmdlet,
            bool respectRateLimit = false,
            TimeSpan? rateLimitDelay = null)
        {
            using var response = client.SendWithErrorHandling(request, cmdlet, respectRateLimit, rateLimitDelay);

            if (response == null || !response.IsSuccessStatusCode)
            {
                return default;
            }

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            try
            {
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Logger.Error(cmdlet, $"Error deserializing response: {ex.Message}");
                return default;
            }
        }
    }
}



