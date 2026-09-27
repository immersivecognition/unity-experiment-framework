using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UXF.Tests
{

	public class TestTrackers
	{

        GameObject gameObject;
        Session session;
        FileSaver fileSaver;
        SessionLogger sessionLogger;
		List<GameObject> tracked = new List<GameObject>();

        [OneTimeSetUp]
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

            session.dataHandlers = new DataHandler[]{ fileSaver };

            sessionLogger.Initialise();
            
            fileSaver.StoragePath = "example_output";
            fileSaver.verboseDebug = true;

            string experimentName = "unit_test";
            string ppid = "test_trackers";
            session.Begin(experimentName, ppid);


            for (int i = 0; i < 5; i++)
			{
                GameObject trackedObject = new GameObject();
                Tracker tracker = trackedObject.AddComponent<PositionRotationTracker>();
				tracker.objectName = string.Format("Tracker_{0}", i);

				session.trackedObjects.Add(tracker);
                tracked.Add(trackedObject);	
			}


			// generate trials
			session.CreateBlock(10);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            session.End();

            GameObject.DestroyImmediate(gameObject);

			foreach (GameObject trackedObject in tracked)
			{
				GameObject.DestroyImmediate(trackedObject);
			}
        }

        [Test]	
        public void TrackManyObjects()
        {
			Random.InitState(1); // reproducible

			foreach (var trial in session.Trials)
			{

				trial.Begin();

				// record 100 times in each trial
				for (int i = 0; i < 100; i++)
				{
                    foreach (GameObject trackedObject in tracked)
                    {
                        trackedObject.transform.position = new Vector3
                        (
                            Random.value, Random.value, Random.value
                        );

                        trackedObject.transform.eulerAngles = new Vector3
                        (
                            Random.value, Random.value, Random.value
                        );
                    
						trackedObject.GetComponent<PositionRotationTracker>().RecordRow();
					}
				}
			
				trial.End();

			}

        }

        [Test]	
        public void AdHocTrackerAdd()
        {
			Random.InitState(2); // reproducible

			foreach (var trial in session.Trials)
			{

				trial.Begin();

                // on each trial, add another gameobject to be tracked
                GameObject newGameObject = new GameObject();
                PositionRotationTracker prt = newGameObject.AddComponent<PositionRotationTracker>();
                prt.objectName = string.Format("adhoc_obj_trial_{0}", trial.number);
                
                session.trackedObjects.Add(prt);
                prt.StartRecording();

				// record 100 times in each trial
				for (int i = 0; i < 100; i++)
				{
                    foreach (Tracker trackedObject in session.trackedObjects)
                    {
                        trackedObject.transform.position = new Vector3
                        (
                            Random.value, Random.value, Random.value
                        );

                        trackedObject.transform.eulerAngles = new Vector3
                        (
                            Random.value, Random.value, Random.value
                        );
                    
						trackedObject.GetComponent<PositionRotationTracker>().RecordRow();
					}
				}

				trial.End();

                session.trackedObjects.Remove(prt);
                GameObject.DestroyImmediate(newGameObject);

			}

        }

        [Test]
        public void RecordingException()
        {
            Tracker testTracker = tracked[0].GetComponent<Tracker>();

            Assert.Throws<System.InvalidOperationException>(() => testTracker.RecordRow());
        }

        [Test]
        public void DuplicateTrackersCausesError()
        {
            string objectName = session.trackedObjects[0].objectName;

            bool errorCaught = false;

            foreach (var trial in session.Trials)
			{
                session.trackedObjects[0].objectName = session.trackedObjects[1].objectName;

				trial.Begin();

                try
                {
                    trial.End();
                }
                catch (System.InvalidOperationException)
                {
                    errorCaught = true;
                }

                session.trackedObjects[0].objectName = objectName;

                trial.End();
            }

            Assert.That(errorCaught);
        }

	}

}