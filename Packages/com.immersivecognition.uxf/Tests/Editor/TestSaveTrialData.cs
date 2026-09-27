using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace UXF.Tests
{
	public class TestSaveTrialData
	{
        
        [Test]
        public void TestDoNotSaveSomeTrials()
        {
            (Session session, FileSaver fileSaver) = CreateSession("DoNotSaveSomeTrials");
            session.blocks[0].trials[0].saveData = false;
            session.blocks[1].saveData = false;

            foreach (var t in session.Trials)
            {
                t.Begin();
                t.End();
            }

            session.End();

            string path = fileSaver.GetSessionPath(session.experimentName, session.ppid, session.number);
            string[] lines = File.ReadAllLines(Path.Combine(path, "trial_results.csv"));
            Assert.AreEqual(2, lines.Length);
        }

        Tuple<Session, FileSaver> CreateSession(string ppidExtra)
        {
            foreach (var logger in UnityEngine.Object.FindObjectsByType<SessionLogger>(FindObjectsSortMode.None))
                GameObject.DestroyImmediate(logger.gameObject);
            foreach (var existingSession in UnityEngine.Object.FindObjectsByType<Session>(FindObjectsSortMode.None))
                GameObject.DestroyImmediate(existingSession.gameObject);
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            if (SessionLogger.instance != null) GameObject.DestroyImmediate(SessionLogger.instance.gameObject);
            // A static reference can outlive a destroyed EditMode object. Clear it before
            // adding a replacement so Session.Awake does not reject the new GameObject.
            Session.instance = null;
            GameObject gameObject = new GameObject();
            // Keep the object inactive while adding components so the test can
            // finish composing the component graph before activation.
            gameObject.SetActive(false);
            Session session = gameObject.AddComponent<Session>();
            FileSaver fileSaver = gameObject.AddComponent<FileSaver>();
            fileSaver.StoragePath = "example_output";

            session.dataHandlers = new DataHandler[]{ fileSaver };
            gameObject.SetActive(true);

            string experimentName = "unit_test";
            string ppid = "test_behaviour_" + ppidExtra;
            session.Begin(experimentName, ppid);
            session.saveData = true;

            // generate trials
			session.CreateBlock(2);
            session.CreateBlock(3);

            return new Tuple<Session, FileSaver>(session, fileSaver);
        }

	}

}
