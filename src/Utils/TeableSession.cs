using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PSTeable.Utils
{
    /// <summary>
    /// Manages the Teable API session state
    /// </summary>
    public class TeableSession
    {
        private static readonly Lazy<TeableSession> _instance = new Lazy<TeableSession>(() => new TeableSession());
        
        /// <summary>
        /// Gets the singleton instance of the TeableSession
        /// </summary>
        public static TeableSession Instance => _instance.Value;
        
        /// <summary>
        /// The HttpClient instance used for all API requests
        /// </summary>
        public HttpClient HttpClient { get; private set; }
        
        /// <summary>
        /// The base URL of the Teable API
        /// </summary>
        public string BaseUrl { get; private set; }
        
        /// <summary>
        /// The API key used for authentication
        /// </summary>
        public string ApiKey { get; private set; }
        
        /// <summary>
        /// Whether the session is connected
        /// </summary>
        public bool IsConnected => !string.IsNullOrEmpty(ApiKey);
        
        private TeableSession()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        /// <summary>
        /// Initializes the session with the provided API key and base URL
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication</param>
        /// <param name="baseUrl">The base URL of the Teable API</param>
        public void Initialize(string apiKey, string baseUrl)
        {
            ApiKey = apiKey;
            BaseUrl = baseUrl.TrimEnd('/');
            
            // Set the Authorization header
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        }
        
        /// <summary>
        /// Clears the session state
        /// </summary>
        public void Clear()
        {
            ApiKey = null;
            BaseUrl = null;
            HttpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
