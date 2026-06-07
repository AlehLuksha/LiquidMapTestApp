using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using LiquidMapTestApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiquidMapTestApp.Helpers
{
    public static class CsvHelper
    {
        private const string FieldSeparator = ",";
        private const char FieldSeparatorChar = ',';
        private const string Quote = "\"";
        private const char QuoteChar = '\"';

        // Constants for size limits to prevent OutOfMemoryException
        private const int MaxCsvDataSizeMB = 100; // 100 MB limit
        private const int MaxCsvDataSizeBytes = MaxCsvDataSizeMB * 1024 * 1024;
        private const int MaxLinesInCsv = 1000000; // 1 million rows

        public static JsonResult ToJson(string fileName, string csvData, int rowsToSkip = 0, bool deleteQuotas = false)
        {
            char[] fieldSeparatorArray = new char[] { FieldSeparatorChar };

            // Validate input size
            if (string.IsNullOrEmpty(csvData))
            {
                throw new ArgumentNullException(nameof(csvData), "Please pass the csv data");
            }

            if (csvData.Length > MaxCsvDataSizeBytes)
            {
                throw new ArgumentException(
                    $"CSV data exceeds maximum allowed size of {MaxCsvDataSizeMB}MB. Current size: {csvData.Length / (1024 * 1024)}MB",
                    nameof(csvData));
            }

            string[] csvLines = ToLines(csvData);

            // Validate line count
            if (csvLines.Length > MaxLinesInCsv)
            {
                throw new ArgumentException(
                    $"CSV data contains too many lines. Maximum allowed: {MaxLinesInCsv}, Current: {csvLines.Length}",
                    nameof(csvData));
            }

            var headers = csvLines[0].Split(fieldSeparatorArray).ToList();

            JsonResult resultSet = new JsonResult(fileName);

            foreach (var line in csvLines.Skip(rowsToSkip))
            {
                //Check to see if a line is blank.
                //This can happen on the last row if improperly terminated.
                if (line != "" || line.Trim().Length > 0)
                {
                    var lineObject = new JObject();
                    var fields = line.Split(fieldSeparatorArray);

                    // Validate field count matches header count
                    if (fields.Length != headers.Count)
                    {
                        throw new InvalidOperationException(
                            $"Row has {fields.Length} fields but expected {headers.Count} based on headers");
                    }

                    for (int x = 0; x < headers.Count; x++)
                    {
                        var value = deleteQuotas ? fields[x].Trim(QuoteChar) : fields[x];
                        lineObject[headers[x]] = value;
                    }

                    resultSet.Rows.Add(lineObject);
                }
            }

            return resultSet;
        }

        public static string FromJson(string jsonData, bool hasHeader = false, bool alwaysQuotas = false)
        {
            // Validate input
            if (string.IsNullOrEmpty(jsonData))
            {
                throw new ArgumentNullException(nameof(jsonData), "Please pass the JSON data");
            }

            if (jsonData.Length > MaxCsvDataSizeBytes)
            {
                throw new ArgumentException(
                    $"JSON data exceeds maximum allowed size of {MaxCsvDataSizeMB}MB",
                    nameof(jsonData));
            }

            dynamic jObj = JsonConvert.DeserializeObject(jsonData);

            if (jObj != null && jObj.Type == JTokenType.Array)
            {
                DataTable table = CsvHelper.JsonStringToTable(jsonData);
                return FromTable(table, hasHeader: true);
            }

            return string.Empty;
        }

        public static string FromTable(DataTable dt, bool hasHeader = false, bool alwaysQuotas = false)
        {
            if (dt == null)
            {
                throw new ArgumentNullException(nameof(dt), "Please pass the list of data");
            }

            // Validate table size
            if (dt.Rows.Count > MaxLinesInCsv)
            {
                throw new ArgumentException(
                    $"DataTable contains too many rows. Maximum allowed: {MaxLinesInCsv}, Current: {dt.Rows.Count}",
                    nameof(dt));
            }

            var properties = new List<string>();
            foreach (DataColumn column in dt.Columns)
            {
                properties.Add(StringToCsvCell(column.ColumnName, alwaysQuotas));
            }

            var result = new StringBuilder();

            if (hasHeader)
            {
                var names = properties;
                var line = string.Join(FieldSeparator, names);
                result.AppendLine(line);
            }

            foreach (DataRow row in dt.Rows)
            {
                var values = new List<string>();
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    values.Add(StringToCsvCell(row[i].ToString(), alwaysQuotas));
                }
                var line = string.Join(FieldSeparator, values);
                result.AppendLine(line);
            }

            return result.ToString();
        }

        public static string FromJson<T>(IList<T> lstData, bool hasHeader = false, bool alwaysQuotas = false)
        {
            if (lstData == null)
            {
                throw new ArgumentNullException(nameof(lstData), "Please pass the list of data");
            }

            // Validate list size
            if (lstData.Count > MaxLinesInCsv)
            {
                throw new ArgumentException(
                    $"List contains too many items. Maximum allowed: {MaxLinesInCsv}, Current: {lstData.Count}",
                    nameof(lstData));
            }

            var properties = typeof(T).GetProperties();
            var result = new StringBuilder();

            if (hasHeader)
            {
                var names = properties.Select(p => p.Name);
                var line = string.Join(FieldSeparator, names);
                result.AppendLine(line);
            }

            foreach (var row in lstData)
            {
                var values = properties.Select(p => p.GetValue(row, null))
                    .Select(v => StringToCsvCell(Convert.ToString(v), alwaysQuotas));
                var line = string.Join(FieldSeparator, values);
                result.AppendLine(line);
            }

            return result.ToString();
        }

        private static string[] ToLines(string dataIn)
        {
            char[] eolMarkerR = new char[] { '\r' };
            char[] eolMarkerN = new char[] { '\n' };
            char[] eolMarker = eolMarkerR;

            //check to see if the file has both \n and \r for end of line markers.
            //common for files comming from Unix\Linux systems.
            if (dataIn.IndexOf('\n') > 0 && dataIn.IndexOf('\r') > 0)
            {
                //if we find both just remove one of them.
                dataIn = dataIn.Replace("\n", "");
            }
            //If the file only has \n then we will use that as the EOL marker to seperate the lines.
            else if (dataIn.IndexOf('\n') > 0)
            {
                eolMarker = eolMarkerN;
            }

            //How do we know the dynamic data will have Split capability?
            return dataIn.Split(eolMarker);
        }

        private static string StringToCsvCell(string str, bool alwaysQuotas = false)
        {
            bool mustQuote = (str.Contains(FieldSeparator) || str.Contains(Quote) || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote || alwaysQuotas)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Quote);
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == QuoteChar)
                        sb.Append(Quote);
                }
                sb.Append(Quote);
                return sb.ToString();
            }

            return str;
        }

        public static DataTable JsonStringToTable(string jsonContent)
        {
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }
    }
}
