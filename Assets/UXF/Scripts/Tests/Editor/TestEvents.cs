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

	public class TestEvents
	{

        GameObject gameObject;
        Session session;
        FileIOManager fileIOManager;
        SessionLogger sessionLogger;

        [SetUp]
        public void SetUp()
        {
            
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void SessionEndEvent()
        {   

            gameObject = new GameObject();
            fileIOManager = gameObject.AddComponent<FileIOManager>();
            sessionLogger = gameObject.AddComponent<SessionLogger>();
            session = gameObject.AddComponent<Session>();

            session.AttachReferences(
                fileIOManager
            );

            sessionLogger.AttachReferences(
                fileIOManager,
                session
            );

            session.onSessionEnd.AddListener(UseSession);

            sessionLogger.Initialise();

            fileIOManager.debug = true;
            fileIOManager.Begin();

            string experimentName = "unit_test";
            string ppid = "test_trials";
            session.Begin(experimentName, ppid, "example_output");
            session.customHeaders.Add("observation");

			// generate trials
			session.CreateBlock(2);
            session.CreateBlock(3);

            int i = 0;
            foreach (var trial in session.Trials)
            {
                trial.Begin();
                trial.result["observation"] = ++i;
                trial.End();
            }

            session.End();




        }


        void UseSession(UXF.Session session)
        {
            int i = 0;
            foreach (var trial in session.Trials)
            {
                Assert.AreEqual(trial.result["observation"], ++i);
            }
        }


	}

}