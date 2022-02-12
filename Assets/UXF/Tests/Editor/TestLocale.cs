using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Globalization;

namespace UXF.Tests
{

    public class TestLocale
    {
        [Test]
        public void TestUS(){ TestSingleLocale(new CultureInfo("en-US")); }

        [Test]
        public void TestGB(){ TestSingleLocale(new CultureInfo("en-GB")); }

        [Test]
        public void TestDE(){ TestSingleLocale(new CultureInfo("de-DE")); }

        public void TestSingleLocale(CultureInfo culture)
        {
            // https://www.csharp-examples.net/culture-names/
            Thread.CurrentThread.CurrentCulture = culture; 

            (Session session, FileSaver fileSaver) = CreateSession(culture.DisplayName);
            session.blocks[0].trials[0].Begin();
            session.blocks[0].trials[0].result["number"] = 123.456f;
            session.blocks[0].trials[0].End();

            string path = fileSaver.GetSessionPath(session.experimentName, session.ppid, session.number);
            session.End();
            GameObject.DestroyImmediate(session.gameObject);

            // read the file to check data
            string[] lines = File.ReadAllLines(Path.Combine(path, "trial_results.csv"));
            var header = lines[0].Split(new string[]{culture.TextInfo.ListSeparator}, StringSplitOptions.None);
            int idx = Array.FindIndex(header, (v) => v == "number");

            var firstRow = lines[1].Split(new string[]{culture.TextInfo.ListSeparator}, StringSplitOptions.None);
            string num = firstRow[idx];

            Assert.AreEqual(123.456f.ToString(), num);

            // json parsing never uses locale, as it does not allow commas within numbers
            var newObject = (Dictionary<string, object>) MiniJSON.Json.Deserialize("{ \"num\": 123.456 }");
            Assert.AreEqual(123.456d, newObject["num"]);
        }

        Tuple<Session, FileSaver> CreateSession(string ppidExtra)
        {
            GameObject gameObject = new GameObject();
            FileSaver fileSaver = gameObject.AddComponent<FileSaver>();
            SessionLogger sessionLogger = gameObject.AddComponent<SessionLogger>();
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            Session session = gameObject.AddComponent<Session>();
            fileSaver.StoragePath = "example_output";

            sessionLogger.AttachReferences(
                session
            );

            session.dataHandlers = new DataHandler[] { fileSaver };

            sessionLogger.Initialise();

            fileSaver.verboseDebug = false;
            fileSaver.forceENUSLocale = false;

            string experimentName = "unit_test";
            string ppid = "test_locale_" + ppidExtra;
            session.Begin(experimentName, ppid);

            // generate trials
            session.CreateBlock(2);
            session.CreateBlock(3);

            return new Tuple<Session, FileSaver>(session, fileSaver);
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB"); 
        }

    }

}