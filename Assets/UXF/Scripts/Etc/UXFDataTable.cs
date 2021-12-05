using System;
using System.Threading;
using System.Globalization;
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
            string[] headers = csvLines[0].Split(',');
            var table = new UXFDataTable(csvLines.Length - 1, headers);

            // traverse down rows
            for (int i = 1; i < csvLines.Length; i++)
            {
                string[] values = csvLines[i].Split(',');

                // if last line, just 1 item in the row, and it is blank, then ignore it
                if (i == csvLines.Length - 1 && values.Length == 1 && values[0].Trim() == string.Empty ) break;

                // check if number of columns is correct
                if (values.Length != headers.Length) throw new Exception($"CSV line {i} has {values.Length} columns, but expected {headers.Length}");

                // build across the row
                var row = new UXFDataRow();
                for (int j = 0; j < values.Length; j++)
                    row.Add((headers[j], values[j].Trim('\"')));

                table.AddCompleteRow(row);
            }

            return table;
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
            string[] lines = new string[CountRows() + 1];
            lines[0] = string.Join(culture.TextInfo.ListSeparator, headers);
            for (int i = 1; i < lines.Length; i++)
            {
                lines[i] = string.Join(culture.TextInfo.ListSeparator,
                    headers
                    .Select(h => FormatItem(dict[h][i - 1], culture, decimalFormat))
                );
            }

            return lines;
        }

        static string FormatItem(object item, CultureInfo culture, string decimalFormat = "0.######")
        {
            switch (item)
            {
                case sbyte sbyteNum:     return sbyteNum.ToString(culture);
                case byte byteNum:       return byteNum.ToString(culture); 
                case short shortNum:     return shortNum.ToString(culture); 
                case ushort ushortNum:   return ushortNum.ToString(culture); 
                case int intNum:         return intNum.ToString(culture);
                case uint uintNum:       return uintNum.ToString(culture);
                case long longNum:       return longNum.ToString(culture);
                case ulong ulongNum:     return ulongNum.ToString(culture);
                case float floatNum:     return floatNum.ToString(decimalFormat, culture);
                case double doubleNum:   return doubleNum.ToString(decimalFormat, culture);
                case decimal decimalNum: return decimalNum.ToString(decimalFormat, culture);
                case null:               return "null";
                default:                 return item.ToString().Replace(culture.TextInfo.ListSeparator, "_");
            };
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
