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

        public static JsonResult ToJson(string fileName, string csvData, int rowsToSkip = 0, bool deleteQuotas = false)
        {
            char[] fieldSeparatorArray = new char[] { FieldSeparatorChar };

            if (csvData == null)
            {
                throw new ArgumentNullException(nameof(csvData), "Please pass the csv data");
            }

            string[] csvLines = ToLines(csvData);

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
                    values.Add(StringToCsvCell(row[i].ToString(),alwaysQuotas));
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
