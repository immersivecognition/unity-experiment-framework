using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF.Tests
{

	public class TestSessionBehaviour
	{

        [Test]
        public void TestEndOnDestroy()
        {
            Session session = CreateSession("endondestroy");
            session.endOnDestroy = true;
            session.blocks[0].trials[0].Begin();
            session.blocks[0].trials[0].End();
            session.blocks[0].trials[1].Begin();

            string path = session.FullPath;
            GameObject.DestroyImmediate(session.gameObject);

            // read the file to check data
            string[] lines = File.ReadAllLines(Path.Combine(session.FullPath, "trial_results.csv"));
            Assert.AreEqual(6, lines.Length);

            // filter blank lines, should only be 1 header + 2 trials
            Assert.AreEqual(
                3,
                lines.Where((line) => !string.IsNullOrEmpty(line)).Count()
            );
        }

        [Test]
        public void TestDontEndOnDestroy()
        {
            Session session = CreateSession("dontendondestroy");
            session.endOnDestroy = false;
            session.blocks[0].trials[0].Begin();
            session.blocks[0].trials[0].End();
            session.blocks[0].trials[1].Begin();

            string path = session.FullPath;
            GameObject.DestroyImmediate(session.gameObject);

            // check csv file didnt get written
            Assert.False(File.Exists(Path.Combine(session.FullPath, "trial_results.csv")));
        }

        [Test]
        public void TestEndAfterLastTrial()
        {
            Session session = CreateSession("endafterlasttrial");
            session.endAfterLastTrial = true;
            session.onTrialEnd.AddListener(session.EndIfLastTrial);

            foreach (var trial in session.Trials)
            {
                trial.Begin();
                trial.End();
            }

            Assert.False(session.hasInitialised);

            // read the file to check data
            string[] lines = File.ReadAllLines(Path.Combine(session.FullPath, "trial_results.csv"));
            Assert.AreEqual(6, lines.Length);
        }

        Session CreateSession(string ppidExtra)
        {
            GameObject gameObject = new GameObject();
            FileIOManager fileIOManager = gameObject.AddComponent<FileIOManager>();
            SessionLogger sessionLogger = gameObject.AddComponent<SessionLogger>();
            Session session = gameObject.AddComponent<Session>();

            session.AttachReferences(
                fileIOManager
            );

            sessionLogger.AttachReferences(
                fileIOManager,
                session
            );

            sessionLogger.Initialise();

            fileIOManager.debug = true;
            fileIOManager.Begin();

            string experimentName = "unit_test";
            string ppid = "test_behaviour_" + ppidExtra;
            session.Begin(experimentName, ppid, "example_output");

            // generate trials
			session.CreateBlock(2);
            session.CreateBlock(3);

            return session;
        }

	}

}