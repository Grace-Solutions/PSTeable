using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace PSTeable.Utils
{
    /// <summary>
    /// Builds URLs for the Teable API
    /// </summary>
    public static class TeableUrlBuilder
    {
        /// <summary>
        /// Builds a URL for the Teable API
        /// </summary>
        /// <param name="path">The path to append to the base URL</param>
        /// <param name="queryParams">Optional query parameters</param>
        /// <returns>The full URL</returns>
        public static string BuildUrl(string path, Dictionary<string, string> queryParams = null)
        {
            if (string.IsNullOrEmpty(TeableSession.Instance.BaseUrl))
            {
                throw new InvalidOperationException("Not connected to Teable. Call Connect-Teable first.");
            }
            
            var baseUrl = TeableSession.Instance.BaseUrl;
            var url = $"{baseUrl}/{path.TrimStart('/')}";
            
            if (queryParams != null && queryParams.Count > 0)
            {
                var queryString = BuildQueryString(queryParams);
                url = $"{url}?{queryString}";
            }
            
            return url;
        }
        
        /// <summary>
        /// Builds a query string from a dictionary of parameters
        /// </summary>
        /// <param name="queryParams">The query parameters</param>
        /// <returns>The query string</returns>
        private static string BuildQueryString(Dictionary<string, string> queryParams)
        {
            var queryString = new StringBuilder();
            
            foreach (var param in queryParams)
            {
                if (queryString.Length > 0)
                {
                    queryString.Append("&");
                }
                
                queryString.Append($"{HttpUtility.UrlEncode(param.Key)}={HttpUtility.UrlEncode(param.Value)}");
            }
            
            return queryString.ToString();
        }
        
        /// <summary>
        /// Gets the URL for spaces
        /// </summary>
        /// <returns>The URL for spaces</returns>
        public static string GetSpacesUrl()
        {
            return BuildUrl("spaces");
        }
        
        /// <summary>
        /// Gets the URL for a specific space
        /// </summary>
        /// <param name="spaceId">The ID of the space</param>
        /// <returns>The URL for the space</returns>
        public static string GetSpaceUrl(string spaceId)
        {
            return BuildUrl($"spaces/{spaceId}");
        }
        
        /// <summary>
        /// Gets the URL for bases in a space
        /// </summary>
        /// <param name="spaceId">The ID of the space</param>
        /// <returns>The URL for bases in the space</returns>
        public static string GetBasesUrl(string spaceId)
        {
            return BuildUrl($"spaces/{spaceId}/bases");
        }
        
        /// <summary>
        /// Gets the URL for a specific base
        /// </summary>
        /// <param name="baseId">The ID of the base</param>
        /// <returns>The URL for the base</returns>
        public static string GetBaseUrl(string baseId)
        {
            return BuildUrl($"bases/{baseId}");
        }
        
        /// <summary>
        /// Gets the URL for tables in a base
        /// </summary>
        /// <param name="baseId">The ID of the base</param>
        /// <returns>The URL for tables in the base</returns>
        public static string GetTablesUrl(string baseId)
        {
            return BuildUrl($"bases/{baseId}/tables");
        }
        
        /// <summary>
        /// Gets the URL for a specific table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <returns>The URL for the table</returns>
        public static string GetTableUrl(string tableId)
        {
            return BuildUrl($"tables/{tableId}");
        }
        
        /// <summary>
        /// Gets the URL for fields in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <returns>The URL for fields in the table</returns>
        public static string GetFieldsUrl(string tableId)
        {
            return BuildUrl($"tables/{tableId}/fields");
        }
        
        /// <summary>
        /// Gets the URL for a specific field
        /// </summary>
        /// <param name="fieldId">The ID of the field</param>
        /// <returns>The URL for the field</returns>
        public static string GetFieldUrl(string fieldId)
        {
            return BuildUrl($"fields/{fieldId}");
        }
        
        /// <summary>
        /// Gets the URL for records in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="viewId">Optional view ID to filter records</param>
        /// <param name="filter">Optional filter expression</param>
        /// <param name="sort">Optional sort expression</param>
        /// <param name="fields">Optional fields to include</param>
        /// <param name="pageSize">Optional page size</param>
        /// <param name="pageToken">Optional page token for pagination</param>
        /// <returns>The URL for records in the table</returns>
        public static string GetRecordsUrl(
            string tableId,
            string viewId = null,
            string filter = null,
            string sort = null,
            string fields = null,
            int? pageSize = null,
            string pageToken = null)
        {
            var queryParams = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(viewId))
            {
                queryParams.Add("viewId", viewId);
            }
            
            if (!string.IsNullOrEmpty(filter))
            {
                queryParams.Add("filter", filter);
            }
            
            if (!string.IsNullOrEmpty(sort))
            {
                queryParams.Add("sort", sort);
            }
            
            if (!string.IsNullOrEmpty(fields))
            {
                queryParams.Add("fields", fields);
            }
            
            if (pageSize.HasValue)
            {
                queryParams.Add("pageSize", pageSize.Value.ToString());
            }
            
            if (!string.IsNullOrEmpty(pageToken))
            {
                queryParams.Add("pageToken", pageToken);
            }
            
            return BuildUrl($"tables/{tableId}/records", queryParams);
        }
        
        /// <summary>
        /// Gets the URL for a specific record
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <param name="recordId">The ID of the record</param>
        /// <returns>The URL for the record</returns>
        public static string GetRecordUrl(string tableId, string recordId)
        {
            return BuildUrl($"tables/{tableId}/records/{recordId}");
        }
        
        /// <summary>
        /// Gets the URL for views in a table
        /// </summary>
        /// <param name="tableId">The ID of the table</param>
        /// <returns>The URL for views in the table</returns>
        public static string GetViewsUrl(string tableId)
        {
            return BuildUrl($"tables/{tableId}/views");
        }
        
        /// <summary>
        /// Gets the URL for a specific view
        /// </summary>
        /// <param name="viewId">The ID of the view</param>
        /// <returns>The URL for the view</returns>
        public static string GetViewUrl(string viewId)
        {
            return BuildUrl($"views/{viewId}");
        }
    }
}

