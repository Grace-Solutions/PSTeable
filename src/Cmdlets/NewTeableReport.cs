using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using PSTeable.Models;
using PSTeable.Utils;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Report type for Teable data
    /// </summary>
    public enum TeableReportType
    {
        /// <summary>
        /// Summary report
        /// </summary>
        Summary,
        
        /// <summary>
        /// Detailed report
        /// </summary>
        Detailed,
        
        /// <summary>
        /// Chart report
        /// </summary>
        Chart,
        
        /// <summary>
        /// Dashboard report
        /// </summary>
        Dashboard
    }
    
    /// <summary>
    /// Creates a new Teable report
    /// </summary>
    [Cmdlet(VerbsCommon.New, "TeableReport")]
    [OutputType(typeof(string))]
    public class NewTeableReport : PSCmdlet
    {
        /// <summary>
        /// The ID of the table to report on
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string TableId { get; set; }
        
        /// <summary>
        /// The type of report to create
        /// </summary>
        [Parameter()]
        public TeableReportType ReportType { get; set; } = TeableReportType.Summary;
        
        /// <summary>
        /// The title of the report
        /// </summary>
        [Parameter()]
        public string Title { get; set; }
        
        /// <summary>
        /// The fields to include in the report
        /// </summary>
        [Parameter()]
        public string[] Fields { get; set; }
        
        /// <summary>
        /// The filter to apply to the records
        /// </summary>
        [Parameter()]
        public TeableFilter Filter { get; set; }
        
        /// <summary>
        /// The sort to apply to the records
        /// </summary>
        [Parameter()]
        public TeableSort Sort { get; set; }
        
        /// <summary>
        /// The field to group by
        /// </summary>
        [Parameter()]
        public string GroupBy { get; set; }
        
        /// <summary>
        /// The path to save the report to
        /// </summary>
        [Parameter()]
        public string Path { get; set; }
        
        /// <summary>
        /// Whether to open the report in a browser
        /// </summary>
        [Parameter()]
        public SwitchParameter Show { get; set; }
        
        /// <summary>
        /// Whether to respect rate limits
        /// </summary>
        [Parameter()]
        public SwitchParameter RespectRateLimit { get; set; }
        
        /// <summary>
        /// The delay to use when rate limited
        /// </summary>
        [Parameter()]
        public TimeSpan RateLimitDelay { get; set; } = TimeSpan.FromSeconds(5);
        
        /// <summary>
        /// Processes the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Get the table information
                var tableRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(TeableUrlBuilder.GetTableUrl(TableId)));
                
                var tableResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableResponse<TeableTable>>(
                    tableRequest,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (tableResponse?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Table {TableId} not found"),
                        "TableNotFound",
                        ErrorCategory.ObjectNotFound,
                        TableId));
                    return;
                }
                
                // Set the title if not provided
                if (string.IsNullOrEmpty(Title))
                {
                    Title = $"{tableResponse.Data.Name} Report";
                }
                
                // Get the fields
                var fieldsRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(TeableUrlBuilder.GetFieldsUrl(TableId)));
                
                var fieldsResponse = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableField>>(
                    fieldsRequest,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                if (fieldsResponse?.Data == null)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"Failed to get fields for table {TableId}"),
                        "GetFieldsFailed",
                        ErrorCategory.ConnectionError,
                        TableId));
                    return;
                }
                
                // Filter the fields if specified
                var fields = fieldsResponse.Data;
                if (Fields != null && Fields.Length > 0)
                {
                    fields = fields.Where(f => Fields.Contains(f.Id) || Fields.Contains(f.Name)).ToList();
                }
                
                // Get the records
                var records = GetRecords();
                
                // Generate the report
                string report;
                switch (ReportType)
                {
                    case TeableReportType.Summary:
                        report = GenerateSummaryReport(tableResponse.Data, fields, records);
                        break;
                    
                    case TeableReportType.Detailed:
                        report = GenerateDetailedReport(tableResponse.Data, fields, records);
                        break;
                    
                    case TeableReportType.Chart:
                        report = GenerateChartReport(tableResponse.Data, fields, records);
                        break;
                    
                    case TeableReportType.Dashboard:
                        report = GenerateDashboardReport(tableResponse.Data, fields, records);
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported report type: {ReportType}");
                }
                
                // Save the report to a file if requested
                if (!string.IsNullOrEmpty(Path))
                {
                    File.WriteAllText(Path, report);
                    WriteVerbose($"Report saved to {Path}");
                }
                
                // Open the report in a browser if requested
                if (Show)
                {
                    string tempPath = System.IO.Path.GetTempFileName() + ".html";
                    File.WriteAllText(tempPath, report);
                    
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempPath,
                        UseShellExecute = true
                    });
                }
                
                // Return the report
                WriteObject(report);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "CreateReportFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Gets records from the table
        /// </summary>
        /// <returns>The records</returns>
        private List<TeableRecord> GetRecords()
        {
            var records = new List<TeableRecord>();
            string pageToken = null;
            
            do
            {
                // Build the URL
                string url = TeableUrlBuilder.GetRecordsUrl(
                    TableId,
                    null,
                    Filter?.ToQueryString(),
                    Sort?.ToQueryString(),
                    Fields != null ? string.Join(",", Fields) : null,
                    100, // Page size
                    pageToken);
                
                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                
                // Send the request
                var response = TeableSession.Instance.HttpClient.SendAndDeserialize<TeableListResponse<TeableRecord>>(
                    request,
                    this,
                    RespectRateLimit,
                    RateLimitDelay);
                
                // Check the response
                if (response?.Data != null)
                {
                    records.AddRange(response.Data);
                    pageToken = response.NextPageToken;
                }
                else
                {
                    pageToken = null;
                }
            }
            while (!string.IsNullOrEmpty(pageToken));
            
            return records;
        }
        
        /// <summary>
        /// Generates a summary report
        /// </summary>
        /// <param name="table">The table information</param>
        /// <param name="fields">The fields to include</param>
        /// <param name="records">The records to report on</param>
        /// <returns>The report HTML</returns>
        private string GenerateSummaryReport(TeableTable table, List<TeableField> fields, List<TeableRecord> records)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <title>Teable Summary Report</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        h1 { color: #333; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("        th { background-color: #f2f2f2; }");
            html.AppendLine("        tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{Title}</h1>");
            
            // Add summary information
            html.AppendLine("    <h2>Summary</h2>");
            html.AppendLine("    <table>");
            html.AppendLine("        <tr><th>Table</th><td>" + table.Name + "</td></tr>");
            html.AppendLine("        <tr><th>Records</th><td>" + records.Count + "</td></tr>");
            html.AppendLine("        <tr><th>Fields</th><td>" + fields.Count + "</td></tr>");
            html.AppendLine("        <tr><th>Generated</th><td>" + DateTime.Now + "</td></tr>");
            html.AppendLine("    </table>");
            
            // Add field statistics
            html.AppendLine("    <h2>Field Statistics</h2>");
            html.AppendLine("    <table>");
            html.AppendLine("        <tr><th>Field</th><th>Type</th><th>Non-Empty</th><th>Empty</th><th>% Filled</th></tr>");
            
            foreach (var field in fields)
            {
                int nonEmpty = records.Count(r => r.Fields != null && r.Fields.ContainsKey(field.Id) && r.Fields[field.Id] != null);
                int empty = records.Count - nonEmpty;
                double percentFilled = records.Count > 0 ? (double)nonEmpty / records.Count * 100 : 0;
                
                html.AppendLine($"        <tr><td>{field.Name}</td><td>{field.Type}</td><td>{nonEmpty}</td><td>{empty}</td><td>{percentFilled:F1}%</td></tr>");
            }
            
            html.AppendLine("    </table>");
            
            // Add group by statistics if requested
            if (!string.IsNullOrEmpty(GroupBy))
            {
                var groupField = fields.FirstOrDefault(f => f.Id == GroupBy || f.Name == GroupBy);
                if (groupField != null)
                {
                    html.AppendLine($"    <h2>Grouped by {groupField.Name}</h2>");
                    html.AppendLine("    <table>");
                    html.AppendLine("        <tr><th>Value</th><th>Count</th><th>Percentage</th></tr>");
                    
                    var groups = records
                        .Where(r => r.Fields != null && r.Fields.ContainsKey(groupField.Id))
                        .GroupBy(r => r.Fields[groupField.Id]?.ToString() ?? "null")
                        .OrderByDescending(g => g.Count())
                        .ToList();
                    
                    foreach (var group in groups)
                    {
                        double percentage = (double)group.Count() / records.Count * 100;
                        html.AppendLine($"        <tr><td>{group.Key}</td><td>{group.Count()}</td><td>{percentage:F1}%</td></tr>");
                    }
                    
                    html.AppendLine("    </table>");
                }
            }
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// Generates a detailed report
        /// </summary>
        /// <param name="table">The table information</param>
        /// <param name="fields">The fields to include</param>
        /// <param name="records">The records to report on</param>
        /// <returns>The report HTML</returns>
        private string GenerateDetailedReport(TeableTable table, List<TeableField> fields, List<TeableRecord> records)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <title>Teable Detailed Report</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        h1 { color: #333; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("        th { background-color: #f2f2f2; }");
            html.AppendLine("        tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{Title}</h1>");
            
            // Add summary information
            html.AppendLine("    <h2>Summary</h2>");
            html.AppendLine("    <table>");
            html.AppendLine("        <tr><th>Table</th><td>" + table.Name + "</td></tr>");
            html.AppendLine("        <tr><th>Records</th><td>" + records.Count + "</td></tr>");
            html.AppendLine("        <tr><th>Fields</th><td>" + fields.Count + "</td></tr>");
            html.AppendLine("        <tr><th>Generated</th><td>" + DateTime.Now + "</td></tr>");
            html.AppendLine("    </table>");
            
            // Add records
            html.AppendLine("    <h2>Records</h2>");
            html.AppendLine("    <table>");
            
            // Add header row
            html.Append("        <tr><th>ID</th>");
            foreach (var field in fields)
            {
                html.Append($"<th>{field.Name}</th>");
            }
            html.AppendLine("</tr>");
            
            // Add data rows
            foreach (var record in records)
            {
                html.Append($"        <tr><td>{record.Id}</td>");
                
                foreach (var field in fields)
                {
                    string value = "";
                    if (record.Fields != null && record.Fields.TryGetValue(field.Id, out var fieldValue))
                    {
                        value = fieldValue?.ToString() ?? "";
                    }
                    
                    html.Append($"<td>{value}</td>");
                }
                
                html.AppendLine("</tr>");
            }
            
            html.AppendLine("    </table>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// Generates a chart report
        /// </summary>
        /// <param name="table">The table information</param>
        /// <param name="fields">The fields to include</param>
        /// <param name="records">The records to report on</param>
        /// <returns>The report HTML</returns>
        private string GenerateChartReport(TeableTable table, List<TeableField> fields, List<TeableRecord> records)
        {
            // Check if a group by field is specified
            if (string.IsNullOrEmpty(GroupBy))
            {
                throw new ArgumentException("GroupBy is required for chart reports");
            }
            
            // Find the group by field
            var groupField = fields.FirstOrDefault(f => f.Id == GroupBy || f.Name == GroupBy);
            if (groupField == null)
            {
                throw new ArgumentException($"Field '{GroupBy}' not found");
            }
            
            // Group the records
            var groups = records
                .Where(r => r.Fields != null && r.Fields.ContainsKey(groupField.Id))
                .GroupBy(r => r.Fields[groupField.Id]?.ToString() ?? "null")
                .OrderByDescending(g => g.Count())
                .ToList();
            
            // Extract the labels and values
            var labels = groups.Select(g => g.Key).ToList();
            var values = groups.Select(g => (double)g.Count()).ToList();
            
            // Create the HTML
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <title>Teable Chart Report</title>");
            html.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        h1 { color: #333; }");
            html.AppendLine("        .chart-container { width: 800px; height: 400px; margin: 20px auto; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{Title}</h1>");
            
            // Add summary information
            html.AppendLine("    <div>");
            html.AppendLine($"        <p><strong>Table:</strong> {table.Name}</p>");
            html.AppendLine($"        <p><strong>Records:</strong> {records.Count}</p>");
            html.AppendLine($"        <p><strong>Grouped by:</strong> {groupField.Name}</p>");
            html.AppendLine($"        <p><strong>Generated:</strong> {DateTime.Now}</p>");
            html.AppendLine("    </div>");
            
            // Add the chart
            html.AppendLine("    <div class=\"chart-container\">");
            html.AppendLine("        <canvas id=\"chart\"></canvas>");
            html.AppendLine("    </div>");
            
            // Add the chart script
            html.AppendLine("    <script>");
            html.AppendLine("        var ctx = document.getElementById('chart').getContext('2d');");
            html.AppendLine("        var chart = new Chart(ctx, {");
            html.AppendLine("            type: 'bar',");
            html.AppendLine("            data: {");
            html.AppendLine($"                labels: [{string.Join(", ", labels.Select(l => $"'{l}'"))}],");
            html.AppendLine("                datasets: [{");
            html.AppendLine($"                    label: '{groupField.Name}',");
            html.AppendLine($"                    data: [{string.Join(", ", values)}],");
            html.AppendLine("                    backgroundColor: [");
            html.AppendLine("                        'rgba(255, 99, 132, 0.2)',");
            html.AppendLine("                        'rgba(54, 162, 235, 0.2)',");
            html.AppendLine("                        'rgba(255, 206, 86, 0.2)',");
            html.AppendLine("                        'rgba(75, 192, 192, 0.2)',");
            html.AppendLine("                        'rgba(153, 102, 255, 0.2)',");
            html.AppendLine("                        'rgba(255, 159, 64, 0.2)'");
            html.AppendLine("                    ],");
            html.AppendLine("                    borderColor: [");
            html.AppendLine("                        'rgba(255, 99, 132, 1)',");
            html.AppendLine("                        'rgba(54, 162, 235, 1)',");
            html.AppendLine("                        'rgba(255, 206, 86, 1)',");
            html.AppendLine("                        'rgba(75, 192, 192, 1)',");
            html.AppendLine("                        'rgba(153, 102, 255, 1)',");
            html.AppendLine("                        'rgba(255, 159, 64, 1)'");
            html.AppendLine("                    ],");
            html.AppendLine("                    borderWidth: 1");
            html.AppendLine("                }]");
            html.AppendLine("            },");
            html.AppendLine("            options: {");
            html.AppendLine("                responsive: true,");
            html.AppendLine("                plugins: {");
            html.AppendLine("                    legend: {");
            html.AppendLine("                        position: 'top',");
            html.AppendLine("                    },");
            html.AppendLine("                    title: {");
            html.AppendLine("                        display: true,");
            html.AppendLine($"                        text: '{Title}'");
            html.AppendLine("                    }");
            html.AppendLine("                }");
            html.AppendLine("            }");
            html.AppendLine("        });");
            html.AppendLine("    </script>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        /// <summary>
        /// Generates a dashboard report
        /// </summary>
        /// <param name="table">The table information</param>
        /// <param name="fields">The fields to include</param>
        /// <param name="records">The records to report on</param>
        /// <returns>The report HTML</returns>
        private string GenerateDashboardReport(TeableTable table, List<TeableField> fields, List<TeableRecord> records)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <title>Teable Dashboard Report</title>");
            html.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        h1 { color: #333; }");
            html.AppendLine("        .dashboard { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }");
            html.AppendLine("        .card { border: 1px solid #ddd; border-radius: 5px; padding: 15px; }");
            html.AppendLine("        .card h2 { margin-top: 0; }");
            html.AppendLine("        .chart-container { height: 300px; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("        th { background-color: #f2f2f2; }");
            html.AppendLine("        tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{Title}</h1>");
            
            // Add summary information
            html.AppendLine("    <div class=\"card\">");
            html.AppendLine("        <h2>Summary</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <tr><th>Table</th><td>" + table.Name + "</td></tr>");
            html.AppendLine("            <tr><th>Records</th><td>" + records.Count + "</td></tr>");
            html.AppendLine("            <tr><th>Fields</th><td>" + fields.Count + "</td></tr>");
            html.AppendLine("            <tr><th>Generated</th><td>" + DateTime.Now + "</td></tr>");
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
            
            html.AppendLine("    <div class=\"dashboard\">");
            
            // Add charts for each field
            int chartIndex = 0;
            foreach (var field in fields.Where(f => f.Type == "singleSelect" || f.Type == "multipleSelect" || f.Type == "checkbox"))
            {
                // Group the records by this field
                var groups = records
                    .Where(r => r.Fields != null && r.Fields.ContainsKey(field.Id))
                    .GroupBy(r => r.Fields[field.Id]?.ToString() ?? "null")
                    .OrderByDescending(g => g.Count())
                    .Take(10) // Limit to top 10
                    .ToList();
                
                // Extract the labels and values
                var labels = groups.Select(g => g.Key).ToList();
                var values = groups.Select(g => (double)g.Count()).ToList();
                
                // Add the chart
                html.AppendLine("        <div class=\"card\">");
                html.AppendLine($"            <h2>{field.Name}</h2>");
                html.AppendLine("            <div class=\"chart-container\">");
                html.AppendLine($"                <canvas id=\"chart{chartIndex}\"></canvas>");
                html.AppendLine("            </div>");
                html.AppendLine("            <script>");
                html.AppendLine($"                var ctx{chartIndex} = document.getElementById('chart{chartIndex}').getContext('2d');");
                html.AppendLine($"                var chart{chartIndex} = new Chart(ctx{chartIndex}, {{");
                html.AppendLine("                    type: 'pie',");
                html.AppendLine("                    data: {");
                html.AppendLine($"                        labels: [{string.Join(", ", labels.Select(l => $"'{l}'"))}],");
                html.AppendLine("                        datasets: [{");
                html.AppendLine($"                            label: '{field.Name}',");
                html.AppendLine($"                            data: [{string.Join(", ", values)}],");
                html.AppendLine("                            backgroundColor: [");
                html.AppendLine("                                'rgba(255, 99, 132, 0.2)',");
                html.AppendLine("                                'rgba(54, 162, 235, 0.2)',");
                html.AppendLine("                                'rgba(255, 206, 86, 0.2)',");
                html.AppendLine("                                'rgba(75, 192, 192, 0.2)',");
                html.AppendLine("                                'rgba(153, 102, 255, 0.2)',");
                html.AppendLine("                                'rgba(255, 159, 64, 0.2)'");
                html.AppendLine("                            ],");
                html.AppendLine("                            borderColor: [");
                html.AppendLine("                                'rgba(255, 99, 132, 1)',");
                html.AppendLine("                                'rgba(54, 162, 235, 1)',");
                html.AppendLine("                                'rgba(255, 206, 86, 1)',");
                html.AppendLine("                                'rgba(75, 192, 192, 1)',");
                html.AppendLine("                                'rgba(153, 102, 255, 1)',");
                html.AppendLine("                                'rgba(255, 159, 64, 1)'");
                html.AppendLine("                            ],");
                html.AppendLine("                            borderWidth: 1");
                html.AppendLine("                        }]");
                html.AppendLine("                    },");
                html.AppendLine("                    options: {");
                html.AppendLine("                        responsive: true,");
                html.AppendLine("                        plugins: {");
                html.AppendLine("                            legend: {");
                html.AppendLine("                                position: 'right',");
                html.AppendLine("                            }");
                html.AppendLine("                        }");
                html.AppendLine("                    }");
                html.AppendLine("                });");
                html.AppendLine("            </script>");
                html.AppendLine("        </div>");
                
                chartIndex++;
            }
            
            // Add a recent records table
            html.AppendLine("        <div class=\"card\">");
            html.AppendLine("            <h2>Recent Records</h2>");
            html.AppendLine("            <table>");
            
            // Add header row
            html.Append("                <tr><th>ID</th>");
            foreach (var field in fields.Take(3)) // Limit to first 3 fields
            {
                html.Append($"<th>{field.Name}</th>");
            }
            html.AppendLine("</tr>");
            
            // Add data rows
            foreach (var record in records.Take(5)) // Limit to first 5 records
            {
                html.Append($"                <tr><td>{record.Id}</td>");
                
                foreach (var field in fields.Take(3)) // Limit to first 3 fields
                {
                    string value = "";
                    if (record.Fields != null && record.Fields.TryGetValue(field.Id, out var fieldValue))
                    {
                        value = fieldValue?.ToString() ?? "";
                    }
                    
                    html.Append($"<td>{value}</td>");
                }
                
                html.AppendLine("</tr>");
            }
            
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
            
            html.AppendLine("    </div>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
    }
}
