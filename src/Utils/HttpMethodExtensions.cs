using System.Net.Http;

namespace PSTeable.Utils
{
    /// <summary>
    /// Extensions for HttpMethod
    /// </summary>
    public static class HttpMethodExtensions
    {
        /// <summary>
        /// The PATCH HTTP method
        /// </summary>
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");
    }
}
