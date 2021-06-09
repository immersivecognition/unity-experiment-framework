using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Globalization;

namespace UXF.Tests
{

	public class TestDataTable
	{

        [Test]
        public void DataTableCSV()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB"); 

            var dt = new UXFDataTable("null", "float", "comma", "period");
            var row = new UXFDataRow();
            row.Add(("null", null));
            row.Add(("float", 3.14f));
            row.Add(("comma", "i have, commas"));
            row.Add(("period", "i have, periods"));

            dt.AddCompleteRow(row);

            string expected = string.Join("\n", new string[]{ "null,float,comma,period", "null,3.14,i have_ commas,i have_ periods" });
            string csv = string.Join("\n", dt.GetCSVLines());

            Assert.AreEqual(expected, csv);
        }

	}

}