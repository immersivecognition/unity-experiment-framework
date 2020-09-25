using System;
using System.Linq;
using System.Collections.Generic;

namespace UXF
{
    public class UXFDataTable
    {
        private Dictionary<string, List<object>> dict;

        public UXFDataTable(params string[] columnNames)
        {
            foreach (string colName in columnNames)
            {
                dict.Add(colName, new List<object>());
            }
        }

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
                newRow.Count == dict.Keys.Count;

            if (!sameKeys) throw new InvalidOperationException("The row does not contain values for the same columns as the columns in the table!");

            foreach (var item in newRow)
            {
                dict[item.columnName].Add(item.value);
            }
        }

        public Dictionary<string, List<object>> GetData()
        {
            return dict;
        }
    }

    public class UXFDataRow : List<(string columnName, object value)> { }

}