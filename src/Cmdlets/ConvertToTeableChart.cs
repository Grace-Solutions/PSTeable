using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PSTeable.Cmdlets
{
    /// <summary>
    /// Chart type for Teable data
    /// </summary>
    public enum TeableChartType
    {
        /// <summary>
        /// Bar chart
        /// </summary>
        Bar,
        
        /// <summary>
        /// Line chart
        /// </summary>
        Line,
        
        /// <summary>
        /// Pie chart
        /// </summary>
        Pie,
        
        /// <summary>
        /// Doughnut chart
        /// </summary>
        Doughnut,
        
        /// <summary>
        /// Area chart
        /// </summary>
        Area,
        
        /// <summary>
        /// Scatter chart
        /// </summary>
        Scatter
    }
    
    /// <summary>
    /// Converts Teable data to a chart
    /// </summary>
    [Cmdlet(VerbsData.ConvertTo, "TeableChart")]
    [OutputType(typeof(string))]
    public class ConvertToTeableChart : PSCmdlet
    {
        /// <summary>
        /// The data to chart
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public PSObject[] Data { get; set; }
        
        /// <summary>
        /// The type of chart to create
        /// </summary>
        [Parameter()]
        public TeableChartType ChartType { get; set; } = TeableChartType.Bar;
        
        /// <summary>
        /// The property to use for labels
        /// </summary>
        [Parameter()]
        public string LabelProperty { get; set; } = "Group";
        
        /// <summary>
        /// The property to use for values
        /// </summary>
        [Parameter()]
        public string ValueProperty { get; set; } = "Count";
        
        /// <summary>
        /// The title of the chart
        /// </summary>
        [Parameter()]
        public string Title { get; set; } = "Teable Data Chart";
        
        /// <summary>
        /// The path to save the chart to
        /// </summary>
        [Parameter()]
        public string Path { get; set; }
        
        /// <summary>
        /// Whether to open the chart in a browser
        /// </summary>
        [Parameter()]
        public SwitchParameter Show { get; set; }
        
        /// <summary>
        /// The list of data to chart
        /// </summary>
        private List<PSObject> _data = new List<PSObject>();
        
        /// <summary>
        /// Processes each data item from the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Data != null)
            {
                _data.AddRange(Data);
            }
        }
        
        /// <summary>
        /// Processes the cmdlet after all pipeline input has been processed
        /// </summary>
        protected override void EndProcessing()
        {
            try
            {
                // Extract the labels and values
                var labels = new List<string>();
                var values = new List<double>();
                
                foreach (var item in _data)
                {
                    // Get the label
                    var label = item.Properties[LabelProperty]?.Value?.ToString();
                    if (string.IsNullOrEmpty(label))
                    {
                        continue;
                    }
                    
                    // Get the value
                    var valueObj = item.Properties[ValueProperty]?.Value;
                    if (valueObj == null)
                    {
                        continue;
                    }
                    
                    double value;
                    if (!double.TryParse(valueObj.ToString(), out value))
                    {
                        continue;
                    }
                    
                    labels.Add(label);
                    values.Add(value);
                }
                
                // Create the chart HTML
                string html = GenerateChartHtml(labels, values);
                
                // Save the chart to a file if requested
                if (!string.IsNullOrEmpty(Path))
                {
                    File.WriteAllText(Path, html);
                    WriteVerbose($"Chart saved to {Path}");
                }
                
                // Open the chart in a browser if requested
                if (Show)
                {
                    string tempPath = System.IO.Path.GetTempFileName() + ".html";
                    File.WriteAllText(tempPath, html);
                    
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempPath,
                        UseShellExecute = true
                    });
                }
                
                // Return the HTML
                WriteObject(html);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ConvertToChartFailed",
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
        
        /// <summary>
        /// Generates the HTML for a chart
        /// </summary>
        /// <param name="labels">The labels for the chart</param>
        /// <param name="values">The values for the chart</param>
        /// <returns>The HTML for the chart</returns>
        private string GenerateChartHtml(List<string> labels, List<double> values)
        {
            // Create a random ID for the chart
            string chartId = $"chart_{Guid.NewGuid().ToString().Replace("-", "")}";
            
            // Create the HTML
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("    <title>Teable Chart</title>");
            html.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{Title}</h1>");
            html.AppendLine($"    <canvas id=\"{chartId}\"></canvas>");
            html.AppendLine("    <script>");
            html.AppendLine($"        var ctx = document.getElementById('{chartId}').getContext('2d');");
            html.AppendLine("        var chart = new Chart(ctx, {");
            html.AppendLine($"            type: '{ChartType.ToString().ToLower()}',");
            html.AppendLine("            data: {");
            html.AppendLine($"                labels: [{string.Join(", ", labels.Select(l => $"'{l}'"))}],");
            html.AppendLine("                datasets: [{");
            html.AppendLine($"                    label: '{ValueProperty}',");
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
    }
}
