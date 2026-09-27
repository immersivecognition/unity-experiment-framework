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
        FileSaver fileSaver;
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
            fileSaver = gameObject.AddComponent<FileSaver>();
            sessionLogger = gameObject.AddComponent<SessionLogger>();
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            session = gameObject.AddComponent<Session>();

            sessionLogger.AttachReferences(
                session
            );
            fileSaver.StoragePath = "example_output";

            session.onSessionEnd.AddListener(UseSession);

            sessionLogger.Initialise();

            fileSaver.verboseDebug = true;

            string experimentName = "unit_test";
            string ppid = "test_trials";
            session.Begin(experimentName, ppid);
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