using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PSTeable.Utils
{
    /// <summary>
    /// Helper class for CSV operations
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Parses a CSV line
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <param name="delimiter">The delimiter</param>
        /// <returns>The parsed values</returns>
        public static string[] ParseCsvLine(string line, string delimiter)
        {
            if (string.IsNullOrEmpty(line))
            {
                return new string[0];
            }

            // Check if the line contains quotes
            if (!line.Contains("\""))
            {
                // Simple case: no quotes
                return line.Split(delimiter.ToCharArray());
            }

            // Complex case: handle quotes
            var values = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        // Escaped quote
                        sb.Append('\"');
                        i++;
                    }
                    else
                    {
                        // Toggle quotes
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter[0] && !inQuotes)
                {
                    // End of field
                    values.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    // Normal character
                    sb.Append(c);
                }
            }

            // Add the last field
            values.Add(sb.ToString());

            return values.ToArray();
        }

        /// <summary>
        /// Escapes a field for CSV output
        /// </summary>
        /// <param name="field">The field to escape</param>
        /// <returns>The escaped field</returns>
        public static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return "";
            }

            // If the field contains a comma, newline, or double quote, wrap it in quotes
            if (field.Contains(",") || field.Contains("\n") || field.Contains("\""))
            {
                // Replace double quotes with two double quotes
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        /// <summary>
        /// Reads a CSV file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="hasHeader">Whether the file has a header row</param>
        /// <param name="delimiter">The delimiter</param>
        /// <returns>The parsed data and headers</returns>
        public static (string[] Headers, List<string[]> Rows) ReadCsvFile(string path, bool hasHeader = true, string delimiter = ",")
        {
            // Check if the file exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"CSV file not found: {path}", path);
            }

            // Read the file
            string[] lines = File.ReadAllLines(path);

            // Check if we have any data
            if (lines.Length == 0 || (lines.Length == 1 && hasHeader))
            {
                return (new string[0], new List<string[]>());
            }

            // Parse the header row
            string[] headers;
            int startRow;

            if (hasHeader)
            {
                headers = ParseCsvLine(lines[0], delimiter);
                startRow = 1;
            }
            else
            {
                // No header row, generate headers
                var firstRow = ParseCsvLine(lines[0], delimiter);
                headers = new string[firstRow.Length];
                for (int i = 0; i < firstRow.Length; i++)
                {
                    headers[i] = $"Field{i + 1}";
                }
                startRow = 0;
            }

            // Parse the data rows
            var rows = new List<string[]>();

            for (int i = startRow; i < lines.Length; i++)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                // Parse the line
                string[] values = ParseCsvLine(lines[i], delimiter);
                rows.Add(values);
            }

            return (headers, rows);
        }

        /// <summary>
        /// Writes data to a CSV file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="headers">The headers</param>
        /// <param name="rows">The rows</param>
        /// <param name="delimiter">The delimiter</param>
        public static void WriteCsvFile(string path, string[] headers, IEnumerable<string[]> rows, string delimiter = ",")
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the CSV rows
            var csvRows = new List<string>();

            // Add the header row
            csvRows.Add(string.Join(delimiter, Array.ConvertAll(headers, EscapeCsvField)));

            // Add the data rows
            foreach (var row in rows)
            {
                csvRows.Add(string.Join(delimiter, Array.ConvertAll(row, EscapeCsvField)));
            }

            // Write the file
            File.WriteAllLines(path, csvRows);
        }
    }
}

