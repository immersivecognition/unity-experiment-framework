using System;
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
        /// Add a complete row to the table
        /// </summary>
        /// <param name="newRow"></param>
        public void AddCompleteRow(UXFDataRow newRow)
        {
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
        /// <returns></returns>
        public string[] GetCSVLines()
        {
            string[] headers = Headers;
            string[] lines = new string[CountRows() + 1];
            lines[0] = string.Join(",", headers);
            for (int i = 1; i < lines.Length; i++)
            {
                lines[i] = string.Join(",", 
                    headers
                    .Select(h => dict[h][i - 1].ToString().Replace(",", "_"))
                );
            }

            return lines;
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