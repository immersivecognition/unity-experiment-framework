using System;
using System.Threading;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace UXF
{
    /// <summary>
    /// Represents a table of data. That is, a series of named columns, each column representing a list of data. The lists of data are always the same length.
    /// </summary>
    public class UXFDataTable
    {
        public string[] Headers { get { return dict.Keys.ToArray(); } }
        private Dictionary<string, List<object>> dict;

        /// <summary>
        /// Construct a table with given estimated row capacity and column names.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="columnNames"></param>
        public UXFDataTable(int capacity, params string[] columnNames)
        {
            dict = new Dictionary<string, List<object>>();
            foreach (string colName in columnNames)
            {
                dict.Add(colName, new List<object>(capacity));
            }
        }

        /// <summary>
        /// Construct a table with given column names.
        /// </summary>
        /// <param name="columnNames"></param>
        public UXFDataTable(params string[] columnNames)
        {
            dict = new Dictionary<string, List<object>>();
            foreach (string colName in columnNames)
            {
                dict.Add(colName, new List<object>());
            }
        }

        /// <summary>
        /// Build a table from lines of CSV text.
        /// </summary>
        /// <param name="csvLines"></param>
        /// <returns></returns>
        public static UXFDataTable FromCSV(string[] csvLines)
        {
            if (csvLines == null || csvLines.Length == 0)
                throw new ArgumentException("CSV input must contain a header row.", "csvLines");

            // File.ReadAllLines removes record terminators. Joining before parsing
            // lets quoted fields retain embedded newlines and also preserves the
            // existing API, which accepts an array of source lines.
            string csv = string.Join("\n", csvLines);
            string delimiter = DetectDelimiter(csv);
            List<List<string>> records = ParseCSV(csv, delimiter[0]);

            // A trailing line terminator appears as one empty record. Keep the
            // historical behavior of ignoring that synthetic record.
            if (records.Count > 1 && records[records.Count - 1].Count == 1 &&
                string.IsNullOrEmpty(records[records.Count - 1][0]))
            {
                records.RemoveAt(records.Count - 1);
            }

            if (records.Count == 0 || records[0].Count == 0)
                throw new FormatException("CSV input must contain a header row.");

            string[] headers = records[0].ToArray();
            if (headers.Any(string.IsNullOrEmpty))
                throw new FormatException("CSV headers must not be empty.");

            var table = new UXFDataTable(Math.Max(0, records.Count - 1), headers);
            for (int i = 1; i < records.Count; i++)
            {
                List<string> values = records[i];
                if (values.Count != headers.Length)
                    throw new FormatException($"CSV record {i + 1} has {values.Count} columns, but expected {headers.Length}.");

                var row = new UXFDataRow();
                for (int j = 0; j < values.Count; j++)
                    row.Add((headers[j], values[j]));
                table.AddCompleteRow(row);
            }

            return table;
        }

        static string DetectDelimiter(string csv)
        {
            int commaCount = csv.TakeWhile(c => c != '\n' && c != '\r').Count(c => c == ',');
            int semicolonCount = csv.TakeWhile(c => c != '\n' && c != '\r').Count(c => c == ';');
            return semicolonCount > commaCount ? ";" : ",";
        }

        static List<List<string>> ParseCSV(string csv, char delimiter)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                    continue;
                }

                if (c == '"' && field.Length == 0)
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    record.Add(field.ToString());
                    field.Length = 0;
                }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                    record.Add(field.ToString());
                    field.Length = 0;
                    records.Add(record);
                    record = new List<string>();
                }
                else
                {
                    field.Append(c);
                }
            }

            if (inQuotes)
                throw new FormatException("CSV contains an unterminated quoted field.");

            if (field.Length > 0 || record.Count > 0 || csv.Length == 0)
            {
                record.Add(field.ToString());
                records.Add(record);
            }
            return records;
        }

        /// <summary>
        /// Add a complete row to the table
        /// </summary>
        /// <param name="newRow"></param>
        public void AddCompleteRow(UXFDataRow newRow)
        {
            if (newRow == null) throw new ArgumentNullException("newRow");

            bool sameKeys = (dict
                .Keys
                .All(
                    newRow
                    .Select(item => item.columnName)
                    .Contains
                    ))
                &&
                (newRow.Count == dict.Keys.Count);

            if (!sameKeys)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The row does not contain values for the same columns as the columns in the table!\nTable: {0}\nRow: {1}",
                        string.Join(", ", Headers),
                        string.Join(", ", newRow.Headers)
                        )
                );
            }

            foreach (var item in newRow)
            {
                dict[item.columnName].Add(item.value);
            }
        }

        /// <summary>
        /// Count and return the number of rows.
        /// </summary>
        /// <returns></returns>
        public int CountRows()
        {
            string[] keyArray = dict.Keys.ToArray();
            if (keyArray.Length == 0) return 0;

            return dict[keyArray[0]].Count();
        }

        /// <summary>
        /// Return the table as a set of strings, each string a line a row with comma-seperated values.
        /// </summary>
        /// <param name="formatProvider">Format provider (e.g. CultureInfo for decimal separator). Defaults to current culture.</param>
        /// <returns></returns>
        public string[] GetCSVLines(CultureInfo culture = null, string decimalFormat = "0.######")
        {
            culture = culture ?? Thread.CurrentThread.CurrentCulture;
            string[] headers = Headers;
            string delimiter = culture.TextInfo.ListSeparator;
            string[] lines = new string[CountRows() + 1];
            lines[0] = string.Join(delimiter, headers.Select(h => FormatItem(h, culture, delimiter)));
            for (int i = 1; i < lines.Length; i++)
            {
                lines[i] = string.Join(delimiter,
                    headers
                    .Select(h => FormatItem(dict[h][i - 1], culture, delimiter, decimalFormat))
                );
            }

            return lines;
        }

        static string FormatItem(object item, CultureInfo culture, string delimiter, string decimalFormat = "0.######")
        {
            string value;
            switch (item)
            {
                case sbyte sbyteNum:     value = sbyteNum.ToString(culture); break;
                case byte byteNum:       value = byteNum.ToString(culture); break;
                case short shortNum:     value = shortNum.ToString(culture); break;
                case ushort ushortNum:   value = ushortNum.ToString(culture); break;
                case int intNum:         value = intNum.ToString(culture); break;
                case uint uintNum:       value = uintNum.ToString(culture); break;
                case long longNum:       value = longNum.ToString(culture); break;
                case ulong ulongNum:     value = ulongNum.ToString(culture); break;
                case float floatNum:     value = floatNum.ToString(decimalFormat, culture); break;
                case double doubleNum:   value = doubleNum.ToString(decimalFormat, culture); break;
                case decimal decimalNum: value = decimalNum.ToString(decimalFormat, culture); break;
                case null:               value = "null"; break;
                default:                 value = item.ToString(); break;
            }

            if (value.IndexOf(delimiter, StringComparison.Ordinal) >= 0 ||
                value.IndexOf('"') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0)
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        /// <summary>
        /// Return the table as a dictionary of lists.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<object>> GetAsDictOfList()
        {
            Dictionary<string, List<object>> dictCopy = new Dictionary<string, List<object>>();
            foreach (var kvp in dict)
            {
                dictCopy.Add(kvp.Key, new List<object>(kvp.Value));
            }
            return dictCopy;
        }

        /// <summary>
        /// Return the table as a list of dictionaries.
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetAsListOfDict()
        {
            int numRows = CountRows();
            List<Dictionary<string, object>> listCopy = new List<Dictionary<string, object>>(numRows);

            for (int i = 0; i < numRows; i++)
            {
                listCopy.Add(
                    Headers.ToDictionary(h => h, h => dict[h][i])
                );
            }

            return listCopy;
        }
    }

    /// <summary>
    /// Represents a single row of data. That is, a series of named columns, each column representing a single value.
    /// The row hold a list of named Tuples (columnName and value). This inherits from List, so to add values, create a new UXFDataRow then add Tuples with the Add method.
    /// </summary>
    public class UXFDataRow : List<(string columnName, object value)>
    {
        /// <summary>
        /// Gets trhe headers of the row.
        /// </summary>
        /// <returns>IEnumerable of strings representing the headers of the row.</returns>
        public IEnumerable<string> Headers { get { return this.Select(kvp => kvp.columnName); } }
    }

}
