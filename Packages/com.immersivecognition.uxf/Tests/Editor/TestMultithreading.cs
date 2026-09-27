using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace UXF.Tests
{

	public class TestMultithreading
	{

        GameObject gameObject;
        Session session;
        FileSaver fileSaver;
        SessionLogger sessionLogger;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            fileSaver = gameObject.AddComponent<FileSaver>();
            sessionLogger = gameObject.AddComponent<SessionLogger>();
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            session = gameObject.AddComponent<Session>();

            sessionLogger.AttachReferences(
                session
            );

            fileSaver.StoragePath = "example_output";

            session.dataHandlers = new DataHandler[]{ fileSaver };

            sessionLogger.Initialise();

            fileSaver.verboseDebug = true;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(gameObject);
        }

		[Test]
		public void AddSettingsDuringWriting()
        {
            string experimentName = "unit_test";
            string ppid = "test_trials";
            Settings settings = Settings.empty;

            // initialise settings
            for (int i = 0; i < 10000; i++)
            {
                string key = i.ToString();
                settings.SetValue(key, i);
            }

            session.Begin(experimentName, ppid, settings: settings);
            
            // add lots more during potential writing
            for (int i = 0; i < 10000; i++)
            {
                string key = "_" + i.ToString();
                settings.SetValue(key, i);
            }

            session.End();
        }

        [Test]
        public void TrialWorkerCanRestartAfterWaiting()
        {
            Trial.WaitForTasks();
            int completedTasks = 0;

            Trial.ManageInWorker(() => Interlocked.Increment(ref completedTasks));
            Trial.WaitForTasks();
            Assert.AreEqual(1, completedTasks);

            Trial.ManageInWorker(() => Interlocked.Increment(ref completedTasks));
            Trial.WaitForTasks();
            Assert.AreEqual(2, completedTasks);
        }

	}

}
