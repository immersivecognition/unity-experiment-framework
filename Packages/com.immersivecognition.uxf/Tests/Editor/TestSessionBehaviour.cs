using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UXF.Tests
{

	public class TestSessionBehaviour
	{

        [Test]
        public void UXFRigStartsWithAnEnabledRuntimeSession()
        {
            const string rigPath = "Packages/com.immersivecognition.uxf/Prefabs/[UXF_Rig].prefab";
            GameObject rig = AssetDatabase.LoadAssetAtPath<GameObject>(rigPath);

            Assert.IsNotNull(rig, "The package UXF rig prefab could not be loaded.");
            Session session = rig.GetComponent<Session>();
            Assert.IsNotNull(session, "The UXF rig must contain a Session component.");
            Assert.IsTrue(session.enabled, "The UXF rig Session must be enabled so its runtime lifecycle and singleton registration run.");
            Assert.IsFalse(typeof(Session).IsDefined(typeof(ExecuteInEditMode), inherit: false));

            EventSystemFallback fallback = rig.GetComponentInChildren<EventSystemFallback>(includeInactive: true);
            Assert.IsNotNull(fallback, "The UXF rig must contain the EventSystem fallback.");
            Assert.IsFalse(typeof(EventSystemFallback).IsDefined(typeof(ExecuteInEditMode), inherit: false));
        }

        [Test]
        public void TestEndOnDestroy()
        {
            (Session session, FileSaver fileSaver) = CreateSession("endondestroy");
            session.endOnDestroy = true;
            session.blocks[0].trials[0].Begin();
            session.blocks[0].trials[0].End();
            session.blocks[0].trials[1].Begin();

            string path = fileSaver.GetSessionPath(session.experimentName, session.ppid, session.number);
            GameObject.DestroyImmediate(session.gameObject);

            // read the file to check data
            string[] lines = File.ReadAllLines(Path.Combine(path, "trial_results.csv"));
            Assert.AreEqual(3, lines.Length);
        }

        [Test]
        public void StaticSessionAndLoggerReferencesClearWhenDestroyed()
        {
            foreach (var existingLogger in UnityEngine.Object.FindObjectsByType<SessionLogger>(FindObjectsSortMode.None))
                GameObject.DestroyImmediate(existingLogger.gameObject);
            foreach (var existingSession in UnityEngine.Object.FindObjectsByType<Session>(FindObjectsSortMode.None))
                GameObject.DestroyImmediate(existingSession.gameObject);
            if (Session.instance != null) GameObject.DestroyImmediate(Session.instance.gameObject);
            if (SessionLogger.instance != null) GameObject.DestroyImmediate(SessionLogger.instance.gameObject);
            Session.instance = null;
            GameObject gameObject = new GameObject();
            gameObject.SetActive(false);
            Session session = gameObject.AddComponent<Session>();
            SessionLogger logger = gameObject.AddComponent<SessionLogger>();
            gameObject.SetActive(true);
            // EditMode AddComponent does not invoke Awake for ordinary MonoBehaviours in Unity 6.
            // Invoke both lifecycles explicitly so this test verifies the static contract without
            // requiring runtime components to execute in edit mode.
            typeof(Session)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(session, null);
            typeof(SessionLogger)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(logger, null);
            session.endOnDestroy = false;

            Assert.AreSame(session, Session.instance);
            Assert.AreSame(logger, SessionLogger.instance);

            GameObject.DestroyImmediate(gameObject);

            Assert.IsNull(Session.instance);
            // Unity objects keep a managed wrapper after destruction; use Unity's
            // overloaded equality so a destroyed logger is treated as null.
            Assert.IsTrue(SessionLogger.instance == null);
        }

        [Test]
        public void TestDontEndOnDestroy()
        {
            (Session session, FileSaver fileSaver) = CreateSession("dontendondestroy");
            session.endOnDestroy = false;
            session.blocks[0].trials[0].Begin();
            session.blocks[0].trials[0].End();
            session.blocks[0].trials[1].Begin();

            string path = fileSaver.GetSessionPath(session.experimentName, session.ppid, session.number);
            GameObject.DestroyImmediate(session.gameObject);

            // check csv file didnt get written
            Assert.False(File.Exists(Path.Combine(path, "trial_results.csv")));
        }

        [Test]
        public void TestEndAfterLastTrial()
        {
            (Session session, FileSaver fileSaver) = CreateSession("endafterlasttrial");
            session.endAfterLastTrial = true;
            session.onTrialEnd.AddListener(session.EndIfLastTrial);

            foreach (var trial in session.Trials)
            {
                trial.Begin();
                trial.End();
            }
            string path = fileSaver.GetSessionPath(session.experimentName, session.ppid, session.number);
            Assert.False(session.hasInitialised);

            // read the file to check data
            string[] lines = File.ReadAllLines(Path.Combine(path, "trial_results.csv"));
            Assert.AreEqual(6, lines.Length);
        }


        [Test]
        public void TestEndMultipleTimes()
        {
            (Session session, FileSaver fileSaver) = CreateSession("endmultipletimes");
            foreach (var trial in session.Trials)
            {
                trial.Begin();
                trial.End();
            }
            session.End();
            session.End();
        }

        [Test]
        public void TestCurrentEndTrialOnBeginNext()
        {
            (Session session, FileSaver fileSaver) = CreateSession("endonbegin");
            session.endOnDestroy = true;
            
            foreach (var t in session.Trials)
            {
                t.Begin();
            }

            foreach (var t in session.Trials)
            {
                TrialStatus expectedStatus = t == session.LastTrial ? TrialStatus.InProgress : TrialStatus.Done;
                Assert.AreEqual(t.status, expectedStatus);
            }

            GameObject.DestroyImmediate(session.gameObject);
        }


        [Test]
		public void DuplicateHeaders()
		{
            (Session session, FileSaver fileSaver) = CreateSession("duplicateheaders");

			session.settingsToLog.Add("hello");
			session.settingsToLog.Add("hello");
			session.customHeaders.Add("hello");
			session.customHeaders.Add("hello");

			session.FirstTrial.Begin();
			session.FirstTrial.End();

			session.End();
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

            session.dataHandlers = new DataHandler[]{ fileSaver };

            sessionLogger.Initialise();

            fileSaver.verboseDebug = true;

            string experimentName = "unit_test";
            string ppid = "test_behaviour_" + ppidExtra;
            session.Begin(experimentName, ppid);

            // generate trials
			session.CreateBlock(2);
            session.CreateBlock(3);

            return new Tuple<Session, FileSaver>(session, fileSaver);
        }

	}

}
