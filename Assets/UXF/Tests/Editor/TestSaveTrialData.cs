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
            (Session session, TestableDataHandler dataHandler) = CreateSession("DoNotSaveSomeTrials");
            session.blocks[0].trials[0].saveData = false;
            session.blocks[1].saveData = false;

            foreach (var t in session.Trials)
            {
                t.Begin();
                t.End();
            }

            session.End();

            var results = dataHandler.trialResults;

            Assert.AreEqual(1, results.CountRows());
        }

        Tuple<Session, TestableDataHandler> CreateSession(string ppidExtra)
        {
            GameObject gameObject = new GameObject();
            TestableDataHandler dataHandler = gameObject.AddComponent<TestableDataHandler>();
            SessionLogger sessionLogger = gameObject.AddComponent<SessionLogger>();
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            Session session = gameObject.AddComponent<Session>();
            sessionLogger.AttachReferences(
                session
            );

            session.dataHandlers = new DataHandler[]{ dataHandler };

            sessionLogger.Initialise();

            string experimentName = "unit_test";
            string ppid = "test_behaviour_" + ppidExtra;
            session.Begin(experimentName, ppid);

            // generate trials
			session.CreateBlock(2);
            session.CreateBlock(3);

            return new Tuple<Session, TestableDataHandler>(session, dataHandler);
        }

	}

}