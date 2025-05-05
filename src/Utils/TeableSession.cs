using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Runtime.InteropServices;

namespace PSTeable.Utils
{
    /// <summary>
    /// Manages the Teable API session state
    /// </summary>
    public class TeableSession : IDisposable
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
        /// The name of the current connection profile
        /// </summary>
        public string CurrentProfileName { get; private set; }

        /// <summary>
        /// Whether the session is connected
        /// </summary>
        public bool IsConnected => !string.IsNullOrEmpty(ApiKey);

        /// <summary>
        /// Dictionary of active connections
        /// </summary>
        private readonly Dictionary<string, (HttpClient Client, string BaseUrl, string ApiKey)> _connections;

        /// <summary>
        /// Flag to track whether Dispose has been called
        /// </summary>
        private bool _disposed;

        private TeableSession()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _connections = new Dictionary<string, (HttpClient, string, string)>();
        }

        /// <summary>
        /// Initializes the session with the provided API key and base URL
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication</param>
        /// <param name="baseUrl">The base URL of the Teable API</param>
        /// <param name="profileName">Optional profile name for the connection</param>
        public void Initialize(string apiKey, string baseUrl, string profileName = null)
        {
            ApiKey = apiKey;
            BaseUrl = baseUrl.TrimEnd('/');
            CurrentProfileName = profileName;

            // Set the Authorization header
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            // If a profile name is provided, store the connection
            if (!string.IsNullOrEmpty(profileName))
            {
                StoreConnection(profileName, HttpClient, BaseUrl, ApiKey);

                // Update the last used timestamp for the profile
                TeableProfileManager.UpdateLastUsed(profileName);
            }
        }

        /// <summary>
        /// Initializes the session with a connection profile
        /// </summary>
        /// <param name="profileName">The name of the profile to use</param>
        public void InitializeWithProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentException("Profile name cannot be empty", nameof(profileName));
            }

            var profile = TeableProfileManager.GetProfile(profileName);
            if (profile == null)
            {
                throw new ArgumentException($"Profile '{profileName}' not found", nameof(profileName));
            }

            // Convert the secure string token to a string
            string apiKey = SecureStringToString(profile.Token);

            // Initialize the session
            Initialize(apiKey, profile.BaseUrl, profileName);

            // Update the last used timestamp for the profile
            TeableProfileManager.UpdateLastUsed(profileName);
        }

        /// <summary>
        /// Switches to a different connection
        /// </summary>
        /// <param name="profileName">The name of the profile to switch to</param>
        public void SwitchConnection(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentException("Profile name cannot be empty", nameof(profileName));
            }

            // Check if the connection exists
            if (!_connections.ContainsKey(profileName))
            {
                // If not, initialize with the profile
                InitializeWithProfile(profileName);
                return;
            }

            // Get the connection
            var (client, baseUrl, apiKey) = _connections[profileName];

            // Update the current session
            HttpClient = client;
            BaseUrl = baseUrl;
            ApiKey = apiKey;
            CurrentProfileName = profileName;

            // Update the last used timestamp for the profile
            TeableProfileManager.UpdateLastUsed(profileName);
        }

        /// <summary>
        /// Gets a list of all active connections
        /// </summary>
        /// <returns>A list of connection profile names</returns>
        public List<string> GetActiveConnections()
        {
            return new List<string>(_connections.Keys);
        }

        /// <summary>
        /// Clears the session state
        /// </summary>
        public void Clear()
        {
            ApiKey = null;
            BaseUrl = null;
            CurrentProfileName = null;
            HttpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Removes a connection
        /// </summary>
        /// <param name="profileName">The name of the profile to remove</param>
        public void RemoveConnection(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentException("Profile name cannot be empty", nameof(profileName));
            }

            // Check if the connection exists
            if (!_connections.ContainsKey(profileName))
            {
                return;
            }

            // If this is the current connection, clear it
            if (profileName == CurrentProfileName)
            {
                Clear();
            }

            // Remove the connection
            _connections.Remove(profileName);
        }

        /// <summary>
        /// Tests the current connection
        /// </summary>
        /// <returns>True if the connection is valid, false otherwise</returns>
        public bool TestConnection()
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                // Create a request to the spaces endpoint
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(TeableUrlBuilder.GetSpacesUrl()));

                // Send the request
                var response = HttpClient.SendAsync(request).GetAwaiter().GetResult();

                // Check if the request was successful
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stores a connection
        /// </summary>
        /// <param name="profileName">The name of the profile</param>
        /// <param name="client">The HttpClient instance</param>
        /// <param name="baseUrl">The base URL</param>
        /// <param name="apiKey">The API key</param>
        private void StoreConnection(string profileName, HttpClient client, string baseUrl, string apiKey)
        {
            _connections[profileName] = (client, baseUrl, apiKey);
        }

        /// <summary>
        /// Converts a SecureString to a string
        /// </summary>
        /// <param name="secureString">The SecureString to convert</param>
        /// <returns>The string value</returns>
        private static string SecureStringToString(SecureString secureString)
        {
            if (secureString == null)
            {
                return null;
            }

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToBSTR(secureString);
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ptr);
                }
            }
        }

        /// <summary>
        /// Disposes the session
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the session
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources
                HttpClient?.Dispose();
                HttpClient = null;
            }

            // Free unmanaged resources

            _disposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~TeableSession()
        {
            Dispose(false);
        }
    }
}

