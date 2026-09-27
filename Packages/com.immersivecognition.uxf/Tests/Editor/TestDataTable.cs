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
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");

                var dt = new UXFDataTable("null", "float", "comma", "period");
                var row = new UXFDataRow();
                row.Add(("null", null));
                row.Add(("float", 3.14f));
                row.Add(("comma", "i have, commas"));
                row.Add(("period", "i have, periods"));

                dt.AddCompleteRow(row);

                string expected = string.Join("\n", new string[]{ "null,float,comma,period", "null,3.14,\"i have, commas\",\"i have, periods\"" });
                string csv = string.Join("\n", dt.GetCSVLines());

                Assert.AreEqual(expected, csv);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void DataTableCSVQuotesAndNewlinesRoundTrip()
        {
            var dt = new UXFDataTable("text", "quote");
            var row = new UXFDataRow();
            row.Add(("text", "line one,\nline two"));
            row.Add(("quote", "say \"hello\""));
            dt.AddCompleteRow(row);

            var restored = UXFDataTable.FromCSV(dt.GetCSVLines());
            var restoredRow = restored.GetAsListOfDict()[0];
            Assert.AreEqual("line one,\nline two", restoredRow["text"]);
            Assert.AreEqual("say \"hello\"", restoredRow["quote"]);
        }

        [Test]
        public void DataTableCSVRejectsMalformedQuotes()
        {
            Assert.Throws<FormatException>(() => UXFDataTable.FromCSV(new[]
            {
                "header,other",
                "\"unterminated,value"
            }));
        }

	}

}
