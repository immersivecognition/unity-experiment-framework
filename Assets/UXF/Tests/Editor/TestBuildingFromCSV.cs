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
            UXFDataTable table = new UXFDataTable("some_text", "an_integer", "a_float", "a_bool_lower", "a_bool_pascal", "a_bool_upper");

            for (int i = 0; i < 10; i++)
            {
                var row = new UXFDataRow();
                row.Add(("some_text", "hello"));
                row.Add(("an_integer", "123"));
                row.Add(("a_float", "3.14"));
                row.Add(("a_bool_lower", "true"));
                row.Add(("a_bool_pascal", "True"));
                row.Add(("a_bool_upper", "TRUE"));

                table.AddCompleteRow(row);
            }

            session.BuildFromTable(table);
            Assert.AreEqual(10, session.Trials.Count());
            Assert.AreEqual(1, session.blocks.Count);

            foreach (var trial in session.Trials)
            {
                Assert.AreEqual("hello", trial.settings.GetString("some_text"));
                Assert.AreEqual(123, trial.settings.GetInt("an_integer"));
                Assert.AreEqual(3.14f, trial.settings.GetFloat("a_float"));
                Assert.AreEqual(true, trial.settings.GetBool("a_bool_lower"));
                Assert.AreEqual(true, trial.settings.GetBool("a_bool_pascal"));
                Assert.AreEqual(true, trial.settings.GetBool("a_bool_upper"));
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

            session.BuildFromTable(table);
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

        [Test]
        public void BuildTableFromCSVWithBlankEntries()
        {
            string testCsv = "some_text,an_integer,a_float\n" +
                "hello,123,3.14\n" +
                "hello,,3.14\n" +
                "hello,123,3.14\n";

            string[] csvLines = testCsv.Split('\n');

            // build a table from the CSV
            UXFDataTable table = UXFDataTable.FromCSV(csvLines);

            // test that the table contains the correct data
            Assert.AreEqual(3, table.Headers.Count());

            // check row 2
            var rows = table.GetAsListOfDict();
            Assert.AreEqual(string.Empty, rows[1]["an_integer"]);
        }

        [Test]
        public void BuildFromTableWithBlankEntries()
        {
            UXFDataTable table = new UXFDataTable("an_integer", "some_text", "a_float");

            var row1 = new UXFDataRow();
            row1.Add(("an_integer", "2"));
            row1.Add(("some_text", "trial_string"));
            row1.Add(("a_float", "3.14"));
            table.AddCompleteRow(row1);

            var row2 = new UXFDataRow();
            row2.Add(("an_integer", "2"));
            row2.Add(("some_text", "")); // blank entry
            row2.Add(("a_float", "3.14"));
            table.AddCompleteRow(row2);

            var row3 = new UXFDataRow();
            row3.Add(("an_integer", "2"));
            row3.Add(("some_text", "    ")); // blank entry with whitespace
            row3.Add(("a_float", "3.14"));
            table.AddCompleteRow(row3);
            
            session.BuildFromTable(table);

            // set a session setting. session needs to be started to create session settings
            session.Begin("test", "test_ppid");
            session.settings.SetValue("some_text", "session_string");

            // should be 3 trials
            Assert.AreEqual(3, session.Trials.Count());

            // pull out the text
            string trial1text = session.GetTrial(1).settings.GetString("some_text");
            string trial2text = session.GetTrial(2).settings.GetString("some_text");
            string trial3text = session.GetTrial(3).settings.GetString("some_text");

            // check that the trial setting is correct
            Assert.AreEqual("trial_string", trial1text);

            // check that the blank entry was ignored in place of the session setting
            Assert.AreEqual("session_string", trial2text);

            // and the whitespace was also ignored
            Assert.AreEqual("session_string", trial3text);
        }


        [Test]
        public void BuildTableFromCSVWithoutBlankLastLine()
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
                "hello,123,3.14";

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