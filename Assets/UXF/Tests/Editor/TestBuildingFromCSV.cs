using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UXF.Tests
{
    public class TestBuildingFromCSV
    {
        GameObject gameObject;
        Session session;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            session = gameObject.AddComponent<Session>();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(gameObject);
        }

        [Test]
        public void BuildFromTable()
        {
            UXFDataTable table = new UXFDataTable("some_text", "an_integer", "a_float");

            for (int i = 0; i < 10; i++)
            {
                var row = new UXFDataRow();
                row.Add(("some_text", "hello"));
                row.Add(("an_integer", "123"));
                row.Add(("a_float", "3.14"));

                table.AddCompleteRow(row);
            }

            session.TryBuildFromTable(table);
			Assert.AreEqual(10, session.Trials.Count());
			Assert.AreEqual(1, session.blocks.Count);

            foreach (var trial in session.Trials)
            {
				Assert.AreEqual("hello", trial.settings.GetString("some_text"));
				Assert.AreEqual(123, trial.settings.GetInt("an_integer"));
				Assert.AreEqual(3.14f, trial.settings.GetFloat("a_float"));
            }

        }

        [Test]
        public void BuildFromTableWithBlockNum()
        {
            UXFDataTable table = new UXFDataTable("block_num", "some_text", "an_integer", "a_float");

            for (int i = 0; i < 10; i++)
            {
                var row = new UXFDataRow();
                row.Add(("block_num", "1"));
                row.Add(("some_text", "hello"));
                row.Add(("an_integer", "123"));
                row.Add(("a_float", "3.14"));

                table.AddCompleteRow(row);
            }

            for (int i = 0; i < 10; i++)
            {
                var row = new UXFDataRow();
                row.Add(("block_num", "2"));
                row.Add(("some_text", "hello"));
                row.Add(("an_integer", "123"));
                row.Add(("a_float", "3.14"));

                table.AddCompleteRow(row);
            }

            session.TryBuildFromTable(table);
			Assert.AreEqual(20, session.Trials.Count());
			Assert.AreEqual(2, session.blocks.Count);

            foreach (var trial in session.Trials)
            {
                Assert.IsFalse(trial.settings.ContainsKey("block_num"));
                Assert.AreEqual("hello", trial.settings.GetString("some_text"));
				Assert.AreEqual(123, trial.settings.GetInt("an_integer"));
				Assert.AreEqual(3.14f, trial.settings.GetFloat("a_float"));
            }

        }

		[Test]
		public void BuildTableFromCSV()
		{
            string testCsv = "some_text,an_integer,a_float\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n" +
                "hello,123,3.14\n";

			string[] csvLines = testCsv.Split('\n');

            // build a table from the CSV
            UXFDataTable table = UXFDataTable.FromCSV(csvLines);

			// test that the table contains the correct data
			Assert.AreEqual(3, table.Headers.Count());

            var rows = table.GetAsListOfDict();
            foreach (var row in rows)
			{
				Assert.AreEqual("hello", row["some_text"]);
				Assert.AreEqual("123", row["an_integer"]);
				Assert.AreEqual("3.14", row["a_float"]);
			}            
        }

    }

}