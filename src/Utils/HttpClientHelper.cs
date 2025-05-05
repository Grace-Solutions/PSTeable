using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PSTeable.Models;

namespace PSTeable.Utils
{
    /// <summary>
    /// Helper class for HTTP operations
    /// </summary>
    public static class HttpClientHelper
    {
        /// <summary>
        /// Common HTTP status code descriptions
        /// </summary>
        private static readonly Dictionary<HttpStatusCode, string> StatusCodeDescriptions = new Dictionary<HttpStatusCode, string>
        {
            { HttpStatusCode.BadRequest, "The request was invalid or cannot be otherwise served." },
            { HttpStatusCode.Unauthorized, "Authentication credentials were missing or incorrect." },
            { HttpStatusCode.Forbidden, "The request is understood, but it has been refused or access is not allowed." },
            { HttpStatusCode.NotFound, "The requested resource could not be found." },
            { HttpStatusCode.MethodNotAllowed, "The request method is not supported for the requested resource." },
            { HttpStatusCode.NotAcceptable, "The requested resource is capable of generating only content not acceptable according to the Accept headers sent in the request." },
            { HttpStatusCode.Conflict, "The request could not be completed due to a conflict with the current state of the resource." },
            { HttpStatusCode.Gone, "The requested resource is no longer available." },
            { HttpStatusCode.LengthRequired, "The server requires a Content-Length header." },
            { HttpStatusCode.PreconditionFailed, "The server does not meet one of the preconditions specified in the request." },
            { HttpStatusCode.RequestEntityTooLarge, "The request is larger than the server is willing or able to process." },
            { HttpStatusCode.RequestUriTooLong, "The URI provided was too long for the server to process." },
            { HttpStatusCode.UnsupportedMediaType, "The server does not support the media type provided in the request." },
            { HttpStatusCode.RequestedRangeNotSatisfiable, "The client has asked for a portion of the file, but the server cannot supply that portion." },
            { HttpStatusCode.ExpectationFailed, "The server cannot meet the requirements of the Expect request-header field." },
            { HttpStatusCode.InternalServerError, "The server encountered an unexpected condition which prevented it from fulfilling the request." },
            { HttpStatusCode.NotImplemented, "The server does not support the functionality required to fulfill the request." },
            { HttpStatusCode.BadGateway, "The server, while acting as a gateway or proxy, received an invalid response from the upstream server." },
            { HttpStatusCode.ServiceUnavailable, "The server is currently unavailable (because it is overloaded or down for maintenance)." },
            { HttpStatusCode.GatewayTimeout, "The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server." },
            { HttpStatusCode.HttpVersionNotSupported, "The server does not support the HTTP protocol version used in the request." },
            { (HttpStatusCode)429, "Too many requests. Rate limit exceeded." }
        };

        /// <summary>
        /// Sends an HTTP request and handles errors
        /// </summary>
        /// <param name="client">The HTTP client</param>
        /// <param name="request">The HTTP request</param>
        /// <param name="cmdlet">The cmdlet making the request</param>
        /// <param name="respectRateLimit">Whether to respect rate limits</param>
        /// <param name="rateLimitDelay">The delay to use when rate limited</param>
        /// <returns>The HTTP response</returns>
        public static HttpResponseMessage SendWithErrorHandling(
            this HttpClient client,
            HttpRequestMessage request,
            PSCmdlet cmdlet,
            bool respectRateLimit = false,
            TimeSpan? rateLimitDelay = null)
        {
            try
            {
                // Send the request
                var response = client.SendAsync(request).GetAwaiter().GetResult();

                // Check for rate limiting
                if (respectRateLimit && response.StatusCode == (HttpStatusCode)429)
                {
                    // Get the retry-after header
                    int retryAfter = 5;
                    if (response.Headers.TryGetValues("Retry-After", out var values))
                    {
                        foreach (var value in values)
                        {
                            if (int.TryParse(value, out int seconds))
                            {
                                retryAfter = seconds;
                                break;
                            }
                        }
                    }

                    // Use the specified delay if provided
                    if (rateLimitDelay.HasValue)
                    {
                        retryAfter = (int)rateLimitDelay.Value.TotalSeconds;
                    }

                    // Log the rate limiting
                    Logger.Verbose(cmdlet, $"Rate limited. Waiting {retryAfter} seconds before retrying...");

                    // Wait for the specified time
                    Thread.Sleep(retryAfter * 1000);

                    // Retry the request
                    request = CloneHttpRequestMessage(request);
                    return SendWithErrorHandling(client, request, cmdlet, respectRateLimit, rateLimitDelay);
                }

                // Check for errors
                if (!response.IsSuccessStatusCode)
                {
                    // Get the status code description
                    string description = GetStatusCodeDescription(response.StatusCode);

                    // Try to get the error message from the response
                    string errorMessage = $"HTTP {(int)response.StatusCode} {response.StatusCode}: {description}";
                    string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        try
                        {
                            // Try to parse the response as JSON
                            using var document = JsonDocument.Parse(responseContent);
                            var root = document.RootElement;

                            // Check for error message
                            if (root.TryGetProperty("error", out var errorElement))
                            {
                                if (errorElement.ValueKind == JsonValueKind.String)
                                {
                                    errorMessage += $" - {errorElement.GetString()}";
                                }
                                else if (errorElement.TryGetProperty("message", out var messageElement))
                                {
                                    errorMessage += $" - {messageElement.GetString()}";
                                }
                            }
                            else if (root.TryGetProperty("message", out var messageElement))
                            {
                                errorMessage += $" - {messageElement.GetString()}";
                            }
                        }
                        catch
                        {
                            // If we can't parse the response as JSON, just include the raw content
                            if (responseContent.Length > 100)
                            {
                                errorMessage += $" - {responseContent.Substring(0, 100)}...";
                            }
                            else
                            {
                                errorMessage += $" - {responseContent}";
                            }
                        }
                    }

                    // Log the error
                    Logger.Verbose(cmdlet, errorMessage);

                    // Throw an exception for 4xx and 5xx errors
                    if ((int)response.StatusCode >= 400)
                    {
                        throw new HttpRequestException(errorMessage);
                    }
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                // Rethrow HTTP request exceptions
                throw;
            }
            catch (Exception ex)
            {
                // Wrap other exceptions
                throw new HttpRequestException($"Error sending HTTP request: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends an HTTP request and deserializes the response
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="client">The HTTP client</param>
        /// <param name="request">The HTTP request</param>
        /// <param name="cmdlet">The cmdlet making the request</param>
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
            // Send the request
            using var response = client.SendWithErrorHandling(request, cmdlet, respectRateLimit, rateLimitDelay);

            // Read the response content
            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // Deserialize the response
            try
            {
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error deserializing response: {ex.Message}. Response content: {content}", ex);
            }
        }

        /// <summary>
        /// Gets a description for an HTTP status code
        /// </summary>
        /// <param name="statusCode">The status code</param>
        /// <returns>The description</returns>
        public static string GetStatusCodeDescription(HttpStatusCode statusCode)
        {
            if (StatusCodeDescriptions.TryGetValue(statusCode, out string description))
            {
                return description;
            }

            return "Unknown status code";
        }

        /// <summary>
        /// Clones an HTTP request message
        /// </summary>
        /// <param name="request">The request to clone</param>
        /// <returns>The cloned request</returns>
        private static HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            // Copy the headers
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy the content
            if (request.Content != null)
            {
                var content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                clone.Content = new StringContent(content, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType);

                // Copy the content headers
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Copy the properties
            foreach (var property in request.Properties)
            {
                clone.Properties.Add(property);
            }

            return clone;
        }
    }
}
